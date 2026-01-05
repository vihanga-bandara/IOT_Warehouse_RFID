import json
import time
from datetime import datetime
import subprocess

from mfrc522 import SimpleMFRC522
import RPi.GPIO as GPIO

from azure.iot.device import IoTHubDeviceClient, Message

# ========== GPIO PINS ==========
RED_PIN = 17
GREEN_PIN = 22
BLUE_PIN = 27

GPIO.setmode(GPIO.BCM)
GPIO.setup(RED_PIN, GPIO.OUT)
GPIO.setup(GREEN_PIN, GPIO.OUT)
GPIO.setup(BLUE_PIN, GPIO.OUT)

def set_led(color):
    GPIO.output(RED_PIN, color == "red")
    GPIO.output(GREEN_PIN, color == "green")
    GPIO.output(BLUE_PIN, color == "blue")

# ========== RFID ==========
reader = SimpleMFRC522()

# ========== Azure IoT Hub ==========
CONNECTION_STRING = (
    "HostName=rfid-warehouse-dev-iothub-26phf7ltazvva.azure-devices.net;"
    "DeviceId=rpi-scanner-01;"
    "SharedAccessKey=KEY"
)

device_client = None

print("RFID scanner started. Press Ctrl+C to exit.")

def has_internet():
    """Return True if internet is available, False otherwise."""
    try:
        subprocess.check_call(
            ["ping", "-c", "1", "8.8.8.8"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL
        )
        return True
    except subprocess.CalledProcessError:
        return False

try:
    while True:
        # Always check internet at the start of each loop
        if not has_internet():
            print("No internet connection!")
            set_led("red")
            time.sleep(2)
            continue  # skip scanning until internet returns

        # Internet is available → show blue while waiting for scan
        set_led("blue")
        print("Waiting for RFID tag...")
        tag_id, text = reader.read()
        tag_text = text.strip() if text else ""

        # Connect to IoT Hub if not already connected
        if device_client is None:
            try:
                device_client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING)
                device_client.connect()
            except Exception as e:
                print("Cannot connect to IoT Hub:", e)
                set_led("red")
                time.sleep(2)
                continue

        # Prepare payload exactly like before
        payload = {
            "deviceId": "rpi-scanner-01",
            "rfidUid": tag_text,
            "tagText": tag_text,
            "timestamp": datetime.utcnow().isoformat() + "Z"
        }

        message = Message(json.dumps(payload))
        message.content_type = "application/json"
        message.content_encoding = "utf-8"

        # Try sending → red LED on failure
        try:
            device_client.send_message(message)
            print("Sent to IoT Hub:", payload)
            set_led("green")  # green on success
        except Exception as e:
            print("Error sending to IoT Hub:", e)
            set_led("red")  # red on send failure

        time.sleep(2)

except KeyboardInterrupt:
    print("Stopping...")

finally:
    GPIO.cleanup()
    if device_client:
        try:
            device_client.disconnect()
        except:
            pass

