# IOT Warehouse RFID Project Knowledge Base

## Project Overview
- **Purpose:** RFID-based warehouse inventory management system with IoT integration
- **Architecture:** Three-tier (Frontend Vue.js, Backend ASP.NET Core, Cloud Infrastructure)
- **Deployment:** Azure App Service + Azure SQL + Azure IoT Hub (frontend is built into the backend `wwwroot` for deployment)
- **Status:** Implementation complete; CI/CD + config documented below

## Frontend (Vue.js)
- **Framework:** Vue 3.5.25 with Vite 7.2.7
- **Styling:** Sass/SCSS with custom theming system
- **Pages:** Login, AdminDashboard, AdminTransactionHistory, AdminUserManagement, UserHistory, Kiosk
- **Features:** Dark/Light mode toggle, responsive design, ToolTrackPro branding
- **Color System:** 100+ CSS variables for light/dark themes, WCAG AA+ contrast
- **Mixins:** 50+ reusable SCSS patterns
- **State Management:** Pinia 3.0.4, localStorage for theme
- **Real-time:** SignalR client for Kiosk
- **Dev Server:** Vite default is http://localhost:5173/ (or next free port)
- **API Base URL:** `VITE_API_URL` (see `frontend_web/.env` and `.env.production`)

## Backend (ASP.NET Core)
- **Framework:** ASP.NET Core 8.0
- **ORM:** Entity Framework Core 8.0.11
- **Database:** SQL Server (Azure)
- **Authentication:** JWT Bearer tokens, BCrypt password hashing
- **Real-time:** SignalR hub for Kiosk
- **Controllers:** Auth, Items, Session, Transaction, Scanners
- **Database Tables:** Users, Items, Transactions, Scanners
- **Deployment:** Azure App Service (Free Tier)
- **API Port:** 5218 (dev), 80 (prod)
- **SignalR Hubs:** `/hubs/kiosk` (authenticated), `/hubs/login` (unauthenticated for RFID login flow)
- **Session Gating:** RFID scans only add to cart when a user has an active kiosk dashboard session

### Auth behavior
- **Admin** can log in without a scanner.
- **Non-admin users must provide `scannerName` at login** (scanner is required for kiosk binding).
- Scans are ignored unless user has an active kiosk dashboard session connected to that scanner.

## Database
- **Type:** Azure SQL Database (Basic tier)
- **Server/DB:** Provisioned by Bicep; names include a unique suffix per environment
- **Authentication:** SQL Server authentication
- **Migration Tool:** EF Core migrations
- **Status:** EF Core migrations included in repo

## Infrastructure as Code (Bicep)
- **Language:** Bicep
- **Location:** backend_cloud/iac/main.bicep
- **Azure Resources:** IoT Hub, App Service, SQL Server, SQL Database, Application Insights
- **Configuration:** Parameterized; emits outputs used by CI/CD

## GitHub Workflows (CI/CD)
- **deploy-infra.yml:** Infrastructure deployment
- **deploy.yml:** Application build and deployment
- **Build Backend:** Restore, Build, Test, Publish
- **Build Frontend:** Install, Build (Vite)
- **Deploy:** Copies frontend build into backend `wwwroot/` and deploys to a single App Service

Notes:
- Workflows support manual runs via `workflow_dispatch`.
- App settings/connection strings are applied during infra deployment (Bicep). The app deploy workflow deploys artifacts only.

### Required GitHub secrets (current repo)
- Azure OIDC: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`
- SQL: `SQL_ADMIN_USERNAME`, `SQL_ADMIN_PASSWORD`
- App: `JWT_SECRET_KEY`
- IoT: `IOT_EVENTHUB_CONNECTION_STRING` (Event Hub-compatible endpoint connection string)

## Authentication & Security
- **JWT:** Configurable; `Jwt:SecretKey` is required
- **Password Hashing:** BCrypt.Net-Next
- **CORS:** Configured for Azure domain
- **HTTPS:** Enforced in Azure App Service
- **Token Storage:** Frontend stores JWT in localStorage/session

## Theming System Architecture
- **CSS Variables:** variables.scss
- **Light/Dark Mode:** Layered backgrounds, contrast compliance
- **Theme Toggle:** ThemeToggle.vue
- **Persistence:** localStorage
- **System Preference:** Fallback to system dark mode
- **useTheme Composable:** State management

## API Endpoints (Backend)
- **Base URL (Azure):** From App Service output (see Azure Portal or Bicep outputs)
- **Base URL (Local Dev):** http://localhost:5218
- **Auth:** /api/auth/register, /api/auth/login, /api/auth/bind-scanner
- **Items:** /api/items
- **Transactions:** /api/transaction
- **Sessions:** /api/session
- **Scanners:** /api/scanners (admin only)
- **SignalR Hubs:** /hubs/kiosk, /hubs/login

## File Structure
- **frontend_web/**: Vue app (src/, styles/, components/, composables/, services/)
- **backend_cloud/api/**: ASP.NET Core API (Controllers/, Data/, Models/, Migrations/, Program.cs)
- **.github/workflows/**: Automation (deploy-infra.yml, deploy.yml)
- **backend_cloud/iac/**: Infrastructure as Code (main.bicep)

## Deployment Process Summary
1. Push code to main branch
2. GitHub Actions deploy-infra workflow → Azure resources
3. GitHub Actions deploy workflow → Build and deploy app
4. Azure App Service serves frontend and backend
5. Backend connects to Azure SQL via `DefaultConnection`

## Observability
- **Application Insights** is provisioned by IaC and wired into the API via App Service settings.
- Useful for confirming:
	- API request/response traces
	- background IoT listener startup/errors
	- dependency calls (SQL)

## Key Technologies & Versions
- Vue 3.5.25, Vite 7.2.7, Sass 1.95.1
- ASP.NET Core 8.0, EF Core 8.0.11
- Azure: App Service, SQL Database, IoT Hub
- JWT, BCrypt, SignalR
- GitHub Actions

## Final Status
- Docs reflect the current repo configuration (ports, env vars, CI/CD)

