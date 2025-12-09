# RFID Scanner on Raspberry Pi

This directory contains the Python application for reading RFID tags on a Raspberry Pi and sending them to Azure IoT Hub.

## Hardware Setup

### RC522 RFID Reader Wiring (SPI)

Connect RC522 to Raspberry Pi GPIO:

```
RC522 Pin    RPi GPIO    RPi Pin
VCC          3.3V        Pin 1/17
GND          GND         Pin 6/9
CLK (SCK)    GPIO 11     Pin 23
MOSI         GPIO 10     Pin 19
MISO         GPIO 9      Pin 21
CS (SDA)     GPIO 8      Pin 24
RST          GPIO 22     Pin 15
IRQ          -           (Optional)
```

### Raspberry Pi SPI Enablement

```bash
sudo raspi-config
# Interface Options > SPI > Enable
```

## Installation

### Prerequisites

- Raspberry Pi 4 with Raspberry Pi OS (Bullseye or later)
- Python 3.9+
- SPI enabled on RPi

### Setup

1. Clone/download the project:

```bash
cd ~/rfid-scanner
```

2. Create and activate virtual environment:

```bash
python3 -m venv venv
source venv/bin/activate
```

3. Install dependencies:

```bash
pip install -r requirements.txt
```

4. Configure environment:

Create `.env` file:

```env
# Azure IoT Hub Device Connection String
IOTHUB_DEVICE_CONNECTION_STRING=HostName=<your-iothub>.azure-devices.net;DeviceId=rpi-scanner-01;SharedAccessKey=<key>

# Scanner Configuration
SCANNER_DEVICE_ID=rpi-scanner-01
READ_TIMEOUT=10
RETRY_DELAY=2
```

## Running

### Manual Start

```bash
python rfid_scanner.py
```

### Systemd Service (Automatic Start)

Create `/etc/systemd/system/rfid-scanner.service`:

```ini
[Unit]
Description=RFID Scanner Service
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/rfid-scanner
Environment="PATH=/home/pi/rfid-scanner/venv/bin"
ExecStart=/home/pi/rfid-scanner/venv/bin/python rfid_scanner.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable rfid-scanner.service
sudo systemctl start rfid-scanner.service
```

View logs:

```bash
sudo journalctl -u rfid-scanner.service -f
```

## Message Format

Messages sent to Azure IoT Hub:

```json
{
  "rfidUid": "A1B2C3D4",
  "deviceId": "rpi-scanner-01",
  "timestamp": "2025-12-10T12:30:45.123456Z",
  "messageType": "RfidScan"
}
```

## Troubleshooting

### SPI not working

Check if SPI is enabled:
```bash
lsmod | grep spi
```

### GPIO Permission Error

Add user to gpio group:
```bash
sudo usermod -aG gpio pi
```

### IoT Hub Connection Failed

Verify connection string in `.env`:
- Format: `HostName=<hub>.azure-devices.net;DeviceId=<id>;SharedAccessKey=<key>`
- Device must be registered in IoT Hub

### No Tags Being Read

1. Check RC522 wiring
2. Verify SPI is enabled
3. Test with: `python3 -c "from pirc522 import RFID; r = RFID()"`
4. Check logs: `tail -f /var/log/rfid_scanner.log`

## Development

Run with debug logging:

```bash
DEBUG=1 python rfid_scanner.py
```

## Security Notes

- Store connection string in `.env` file (not in git)
- Use environment variables for sensitive data
- Consider using device certificates instead of connection string
- Rotate access keys regularly

## Performance Tuning

- `READ_TIMEOUT`: Lower = faster response, higher = fewer retries
- `RETRY_DELAY`: Delay between connection retries
- Adjust in `.env` as needed

1. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

2. Run the scanner:
   ```bash
   python rfid_scanner.py
   ```

## Configuration

TODO: Add configuration details
