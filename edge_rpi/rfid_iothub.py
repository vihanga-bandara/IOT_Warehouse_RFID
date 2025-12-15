import json
import time
from datetime import datetime

from mfrc522 import SimpleMFRC522
import RPi.GPIO as GPIO

from azure.iot.device import IoTHubDeviceClient, Message

# ========== Azure IoT Hub ==========
CONNECTION_STRING = (
    "HostName=rfid-warehouse-dev-iothub-26phf7ltazvva.azure-devices.net;"
    "DeviceId=rpi-scanner-01;"
    "SharedAccessKey=KEY"
)
# ========== RFID ==========
reader = SimpleMFRC522()

print("RFID scanner started. Press Ctrl+C to exit.")

# 🔑 CREATE CLIENT (NO 'with')
device_client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING)
device_client.connect()

try:
    while True:
        print("Waiting for RFID tag...")
        tag_id, text = reader.read()

        payload = {
            "deviceId": "rpi-scanner-01",
            "rfidTagId": str(tag_id),
            "tagText": text.strip() if text else "",
            "timestamp": datetime.utcnow().isoformat() + "Z"
        }

        message = Message(json.dumps(payload))
        message.content_type = "application/json"
        message.content_encoding = "utf-8"

        device_client.send_message(message)
        print("Sent to IoT Hub:", payload)

        time.sleep(2)

except KeyboardInterrupt:
    print("Stopping...")

finally:
    GPIO.cleanup()
    device_client.disconnect()
