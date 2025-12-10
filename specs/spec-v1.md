# Project Specification: RFID Warehouse Tool Management System

## 1. Project Overview
This project is a self-service kiosk for borrowing and returning tools in a warehouse environment using RFID technology. The system is designed to run on a strictly "Zero Cost" architecture using Azure Student/Free tiers.

**Core Workflow:**
1. Users log in to a Kiosk terminal.
2. Users scan RFID-tagged tools using a physical reader.
3. Scanned items enter a temporary "Cart" (Session State) to allow review before committing.
4. Users confirm the transaction to either "Borrow" or "Return" items, which updates the database.

Admin can login to separate dashboard for admins and check status the inventory, it should show, borrowed or available, the time and who borrowed it if borrowed.

---

## 2. Architecture & Constraints

### 2.1 Azure Free Tier Constraints (Strict)
* **App Service:** Must use Linux **F1 Tier**. This is shared infrastructure with no "Always On" feature. The application handles cold starts.
* **Database:** Azure SQL Serverless **Free Tier**. Auto-pause must be enabled to preserve free credits.
* **IoT Hub:** **Free Tier** (Max 8,000 messages/day).
* **Frontend Hosting:** No separate Static Web App resource. The Vue.js frontend must be compiled and served as static files (`wwwroot`) directly by the ASP.NET Core API.
* **State Management:** No Redis or external cache allowed due to cost. User session state must be managed in-memory within the API process.

### 2.2 System Components
* **Edge (Warehouse):** Raspberry Pi 4 + RC522 Reader running a Python script. Sends RFID UIDs to Azure IoT Hub via MQTT.
* **Cloud (Azure):**
    * **IoT Hub:** Receives MQTT messages.
    * **App Service (Backend):** Hosts the .NET Web API, the Background MQTT Listener, and serves the Frontend.
    * **SQL Database:** Stores Users, Items, and Transaction logs.
* **Clients:**
    * **Kiosk UI:** Vue.js SPA for scanning and borrowing.
    * **Admin UI:** Vue.js SPA for inventory management and logs.

---

## 3. Technology Stack

### Backend (Monolith)
* **Framework:** ASP.NET Core 8 Web API.
* **Language:** C#.
* **ORM:** Entity Framework Core (Code First).
* **Real-time Communication:** SignalR (Self-hosted within the API process).
* **Background Processing:** `IHostedService` implementation for listening to IoT Hub messages.

### Frontend
* **Framework:** Vue.js 3 (Composition API).
* **Build Tool:** Vite.
* **Routing:** Vue Router (Views: `/kiosk`, `/admin`).
* **State Management:** Pinia (specifically for managing the cart state on the client).

### Infrastructure (IaC)
* **Tool:** Azure Bicep.
* **Target:** strictly F1/Free tier SKUs.

---

## 4. Database Schema
*Note: Use Entity Framework Core conventions.*

### Entities

**1. User**
* `user_id` (PK, int)
* `email` (varchar, Unique)
* `password_hash` (varchar) - *For custom JWT auth*
* `rfid_tag_uid` (varchar) - *For future badge login*
* `name`, `lastname` (varchar)
* `role_id` (FK)

**2. Item**
* `item_id` (PK, int)
* `rfid_uid` (varchar, Unique, Index) - *The physical Tag ID*
* `item_name` (varchar)
* `status` (Enum: `Available`, `Borrowed`, `Lost`, `Maintenance`)
* `current_holder_id` (FK to User, nullable)
* `last_updated` (DateTime)

**3. Transaction (Audit Log)**
* `transaction_id` (PK, int)
* `user_id` (FK)
* `item_id` (FK)
* `device_id` (FK to Scanner)
* `action` (Enum: `Checkout`, `Checkin`)
* `timestamp` (DateTime)
* `notes` (text, nullable)

**4. Scanner**
* `scanner_id` (PK, int)
* `device_id_string` (varchar) - *Must match the Azure IoT Hub Device ID (e.g., "rpi-scanner-01")*
* `location_name` (varchar)

**5. UserRight**
* `user_id` (FK)
* `role_id` (FK)

---

## 5. Core Logic Specifications

### 5.1 The "Cart" Flow (Session Management)
To prevent accidental database writes and handle hardware noise, the system uses a Session/Cart model, not instant commits.

**Backend Service: `CheckoutSessionManager` (Singleton)**
This service manages active user sessions in memory (`ConcurrentDictionary`).

**Logic Flow:**
1.  **Receive Scan:** The MQTT Listener receives an `rfid_uid` and `device_id`.
2.  **Lookup Item:** The system checks the DB for the `rfid_uid`.
3.  **Identify Action:**
    * If Item Status is `Available` -> Action is **Borrow**.
    * If Item Status is `Borrowed` AND `current_holder` is the Current User -> Action is **Return**.
    * If Item Status is `Borrowed` AND `current_holder` is NOT the Current User -> **Error** (Item held by someone else).
4.  **Debounce / Duplicate Prevention:**
    * The system checks if this specific item is *already* in the user's active Cart list.
    * If yes: **IGNORE** the scan (drops duplicate hardware signals).
    * If no: Add to the Cart list in memory.
5.  **SignalR Push:** The system pushes a specific `UpdateCart` message to the logged-in User's frontend client to update the UI.

### 5.2 The "Checkout" Transaction
Database updates happen only when the user confirms the action on the Kiosk.

**Process:**
1.  User clicks "Confirm Transaction".
2.  Backend receives the request for the current session.
3.  **Concurrency Check:** Re-verify DB status for all items in the cart (ensure availability hasn't changed).
4.  **Batch Update:**
    * Update `Item` table: Set `status` to `Borrowed`/`Available` and update `current_holder_id`.
    * Insert rows into `Transaction` table for audit history.
5.  **Clear Session:** The user is removed from the `CheckoutSessionManager`.

### 5.3 Authentication
* **Method:** Custom Email/Password Login returning a JWT (JSON Web Token).
* **Restriction:** Users must be logged in to process both Borrows and Returns to ensure a complete audit trail.

---

## 6. API Endpoints (Minimal Viable)

### Auth
* `POST /api/auth/login` - Returns JWT.
* `POST /api/auth/register` - (Optional for MVP).

### Inventory
* `GET /api/items` - List all items (Admin).
* `GET /api/items/{id}` - Details.
* `POST /api/items` - Register a new tool tag.

### Transaction / Kiosk
* `GET /api/session/current` - Retrieves current in-memory cart (for page refresh).
* `POST /api/session/clear` - Empties the cart without committing.
* `POST /api/transaction/commit` - Finalizes the Borrow/Return process.

---

## 7. Frontend Requirements (Vue.js)

Give special attention to the look and feel of the front-end application (UI UX should be great and professional looking). We already have a logo which you can include as needed.

### Kiosk View
* **Login Screen:** Email/Password form.
* **Scanning Screen:**
    * Displays a real-time list of scanned items via SignalR.
    * Visual distinction: Green for "Items to Borrow", Orange for "Items to Return".
    * "Confirm Transaction" button (Disabled if cart is empty).
    * "Cancel/Clear" button.
* **Past Borrowing History of this user**
    * Displays all items borrowed with dates.

### Admin View
* **Dashboard:** Shows a list of currently borrowed items and who holds them.
* **History:** Shows a table of the Transaction log.
* **Add new users:** Shows a form to add new users, the receptionist or the admin who accepts the admission will rotate the screen and allow the user to type in his/her pw (for mvp this is enough).