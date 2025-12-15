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
    "SharedAccessKey=YOUR_KEY_HERE"
)

# ========== RFID ==========
reader = SimpleMFRC522()

print("RFID scanner started. Press Ctrl+C to exit.")

try:
    with IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING) as device_client:
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

            # Optional message properties (great for routing)
            message.custom_properties["eventType"] = "rfidScan"

            device_client.send_message(message)

            print("Sent to IoT Hub:", payload)
            time.sleep(2)

except KeyboardInterrupt:
    print("Stopping...")

finally:
    GPIO.cleanup()

