# RFID Warehouse API - Default Credentials

## Test Users

### Admin User
- **Email:** `admin@warehouse.com`
- **Password:** `password123`
- **Role:** Admin (RoleId: 1)
- **Access:** Full access to all endpoints including user management and admin dashboard

### Regular User
- **Email:** `john.doe@warehouse.com`
- **Password:** `password123`
- **Role:** User (RoleId: 2)
- **Access:** Kiosk functionality, can borrow/return items

## Test Items

The database is seeded with 5 test items:
1. Power Drill (RFID: RFID001234567890)
2. Hammer (RFID: RFID001234567891)
3. Screwdriver Set (RFID: RFID001234567892)
4. Measuring Tape (RFID: RFID001234567893)
5. Wrench Set (RFID: RFID001234567894)

## Test Scanner

- **Device ID:** `rpi-scanner-01`
- **Location:** Main Warehouse Entrance

## Running Migrations

To apply the database schema:

```bash
cd backend_cloud/api
dotnet ef database update
```

## Notes

⚠️ **IMPORTANT:** Change these default credentials before deploying to production!
