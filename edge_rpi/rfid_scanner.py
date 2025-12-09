"""
RFID Scanner with Azure IoT Hub Integration
Reads RFID tags from RC522 reader and sends to Azure IoT Hub
"""

import logging
import json
import os
import signal
import time
import sys
import threading
from datetime import datetime
from abc import ABC, abstractmethod

import RPi.GPIO as GPIO
from pirc522 import RFID
from dotenv import load_dotenv
from azure.iot.device import IoTHubDeviceClient, Message
from azure.iot.device.exceptions import OperationError

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/var/log/rfid_scanner.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

# Load environment variables
load_dotenv()

class RFIDScanner:
    """RC522 RFID Reader Interface"""
    
    def __init__(self):
        """Initialize RFID scanner"""
        try:
            self.rfc = RFID()
            logger.info("RFID Scanner initialized successfully")
        except Exception as e:
            logger.error(f"Failed to initialize RFID scanner: {e}")
            raise
    
    def read_tag(self, timeout=5):
        """
        Read RFID tag with timeout
        
        Args:
            timeout: Read timeout in seconds
            
        Returns:
            RFID UID string or None if no tag read
        """
        start_time = time.time()
        
        while (time.time() - start_time) < timeout:
            self.rfc.wait_for_tag()
            
            if self.rfc.tag_present():
                status, tag_type = self.rfc.request(self.rfc.const_MIFARE)
                
                if status == self.rfc.const_stat_ok:
                    status, raw_uid = self.rfc.anticoll()
                    
                    if status == self.rfc.const_stat_ok:
                        uid = self._format_uid(raw_uid)
                        logger.info(f"Tag read: {uid}")
                        return uid
            
            time.sleep(0.1)
        
        return None
    
    def _format_uid(self, raw_uid):
        """Format raw UID to hex string"""
        return ''.join(f'{byte:02X}' for byte in raw_uid)
    
    def cleanup(self):
        """Cleanup GPIO"""
        try:
            GPIO.cleanup()
            logger.info("GPIO cleaned up")
        except Exception as e:
            logger.error(f"Error during cleanup: {e}")


class IoTHubConnector:
    """Azure IoT Hub Connection Handler"""
    
    def __init__(self, connection_string):
        """
        Initialize IoT Hub connection
        
        Args:
            connection_string: Azure IoT Hub connection string
        """
        self.connection_string = connection_string
        self.client = None
        self.connected = False
        self.reconnect_attempts = 0
        self.max_reconnect_attempts = 10
        self.reconnect_delay = 5
    
    def connect(self):
        """Connect to Azure IoT Hub"""
        try:
            self.client = IoTHubDeviceClient.create_from_connection_string(
                self.connection_string
            )
            self.client.connect()
            self.connected = True
            self.reconnect_attempts = 0
            logger.info("Connected to Azure IoT Hub")
            return True
        except OperationError as e:
            logger.error(f"Failed to connect to IoT Hub: {e}")
            return False
        except Exception as e:
            logger.error(f"Unexpected error during connection: {e}")
            return False
    
    def send_rfid_scan(self, rfid_uid, device_id):
        """
        Send RFID scan to IoT Hub
        
        Args:
            rfid_uid: RFID tag UID
            device_id: Scanner device ID
            
        Returns:
            True if message sent successfully
        """
        if not self.connected:
            logger.warning("Not connected to IoT Hub, attempting reconnect")
            if not self._reconnect():
                return False
        
        try:
            message_data = {
                'rfidUid': rfid_uid,
                'deviceId': device_id,
                'timestamp': datetime.utcnow().isoformat() + 'Z',
                'messageType': 'RfidScan'
            }
            
            message = Message(
                json.dumps(message_data),
                content_encoding='utf-8',
                content_type='application/json'
            )
            
            self.client.send_message(message)
            logger.info(f"Message sent: {rfid_uid} from {device_id}")
            return True
        
        except OperationError as e:
            logger.error(f"Failed to send message: {e}")
            self.connected = False
            return False
        except Exception as e:
            logger.error(f"Unexpected error sending message: {e}")
            self.connected = False
            return False
    
    def _reconnect(self):
        """Attempt to reconnect to IoT Hub"""
        self.reconnect_attempts += 1
        
        if self.reconnect_attempts > self.max_reconnect_attempts:
            logger.error("Max reconnection attempts reached")
            return False
        
        logger.info(f"Reconnection attempt {self.reconnect_attempts}")
        time.sleep(self.reconnect_delay)
        
        return self.connect()
    
    def disconnect(self):
        """Disconnect from IoT Hub"""
        try:
            if self.client:
                self.client.disconnect()
                self.connected = False
                logger.info("Disconnected from Azure IoT Hub")
        except Exception as e:
            logger.error(f"Error during disconnect: {e}")


class RFIDScannerApplication:
    """Main Application"""
    
    def __init__(self):
        """Initialize application"""
        # Configuration
        self.connection_string = os.getenv(
            'IOTHUB_DEVICE_CONNECTION_STRING',
            ''
        )
        self.device_id = os.getenv(
            'SCANNER_DEVICE_ID',
            'rpi-scanner-01'
        )
        self.read_timeout = int(os.getenv('READ_TIMEOUT', '10'))
        self.retry_delay = int(os.getenv('RETRY_DELAY', '2'))
        
        # Validate configuration
        if not self.connection_string:
            logger.error("IOTHUB_DEVICE_CONNECTION_STRING not set")
            raise ValueError("Missing IoT Hub connection string")
        
        # Initialize components
        self.scanner = None
        self.iot_hub = None
        self.running = False
    
    def start(self):
        """Start the application"""
        logger.info("Starting RFID Scanner Application")
        logger.info(f"Device ID: {self.device_id}")
        
        try:
            # Initialize scanner
            self.scanner = RFIDScanner()
            
            # Initialize IoT Hub connection
            self.iot_hub = IoTHubConnector(self.connection_string)
            if not self.iot_hub.connect():
                logger.error("Failed to connect to IoT Hub on startup")
                return False
            
            self.running = True
            logger.info("Application started successfully")
            return True
        
        except Exception as e:
            logger.error(f"Failed to start application: {e}")
            return False
    
    def run(self):
        """Main scanning loop"""
        if not self.start():
            return
        
        try:
            logger.info("Starting scan loop...")
            
            while self.running:
                try:
                    logger.info(f"Waiting for RFID tag (timeout: {self.read_timeout}s)")
                    
                    # Read RFID tag
                    rfid_uid = self.scanner.read_tag(timeout=self.read_timeout)
                    
                    if rfid_uid:
                        # Send to IoT Hub
                        success = self.iot_hub.send_rfid_scan(rfid_uid, self.device_id)
                        
                        if success:
                            logger.info(f"Successfully processed tag: {rfid_uid}")
                        else:
                            logger.warning(f"Failed to send tag to IoT Hub: {rfid_uid}")
                            # Retry after delay
                            time.sleep(self.retry_delay)
                    else:
                        logger.debug("No tag detected within timeout")
                    
                    # Small delay between reads to prevent CPU spike
                    time.sleep(0.5)
                
                except KeyboardInterrupt:
                    logger.info("Keyboard interrupt received")
                    self.running = False
                
                except Exception as e:
                    logger.error(f"Error in scan loop: {e}")
                    time.sleep(self.retry_delay)
        
        finally:
            self.stop()
    
    def stop(self):
        """Stop the application"""
        logger.info("Stopping RFID Scanner Application")
        self.running = False
        
        try:
            if self.scanner:
                self.scanner.cleanup()
            
            if self.iot_hub:
                self.iot_hub.disconnect()
            
            logger.info("Application stopped successfully")
        except Exception as e:
            logger.error(f"Error during shutdown: {e}")
    
    def signal_handler(self, sig, frame):
        """Handle signals (SIGTERM, SIGINT)"""
        logger.info(f"Received signal {sig}")
        self.stop()
        sys.exit(0)


def main():
    """Application entry point"""
    app = RFIDScannerApplication()
    
    # Register signal handlers
    signal.signal(signal.SIGTERM, app.signal_handler)
    signal.signal(signal.SIGINT, app.signal_handler)
    
    # Run application
    try:
        app.run()
    except Exception as e:
        logger.error(f"Fatal error: {e}")
        sys.exit(1)


if __name__ == '__main__':
    main()

# TODO: Implement RFID scanner functionality

if __name__ == "__main__":
    print("RFID Scanner starting...")
    # Main application code
