# IOT Warehouse RFID Project Knowledge Base

## Project Overview
- **Purpose:** RFID-based warehouse inventory management system with IoT integration
- **Architecture:** Three-tier (Frontend Vue.js, Backend ASP.NET Core, Cloud Infrastructure)
- **Deployment:** Hybrid (Frontend on Azure App Service, Backend on Azure App Service, Database on Azure SQL)
- **Status:** 100% complete – infrastructure, migrations, CORS, and API endpoint configuration all sorted

## Frontend (Vue.js)
- **Framework:** Vue 3.5.25 with Vite 7.2.7
- **Styling:** Sass/SCSS with custom theming system
- **Pages:** Login, AdminDashboard, AdminTransactionHistory, AdminUserManagement, UserHistory, Kiosk
- **Features:** Dark/Light mode toggle, responsive design, ToolTrackPro branding
- **Color System:** 100+ CSS variables for light/dark themes, WCAG AA+ contrast
- **Mixins:** 50+ reusable SCSS patterns
- **State Management:** Pinia 3.0.4, localStorage for theme
- **Real-time:** SignalR client for Kiosk
- **Dev Server:** http://localhost:5174/
- **API Endpoints:** Environment-based (localhost for dev, Azure for prod)

## Backend (ASP.NET Core)
- **Framework:** ASP.NET Core 8.0
- **ORM:** Entity Framework Core 8.0.11
- **Database:** SQL Server (Azure)
- **Authentication:** JWT Bearer tokens, BCrypt password hashing
- **Real-time:** SignalR hub for Kiosk
- **Controllers:** Auth, Items, Session, Transaction
- **Database Tables:** Users, Items, Transactions, Scanners
- **Deployment:** Azure App Service (Free Tier)
- **API Port:** 5218 (dev), 80 (prod)

## Database
- **Type:** Azure SQL Database (Basic tier)
- **Server:** rfid-warehouse-dev-sqlserver-26phf7ltazvva.database.windows.net
- **Database Name:** rfid-warehouse-dev-db
- **Authentication:** SQL Server authentication
- **Migration Tool:** EF Core migrations
- **Status:** Schema applied, migrations complete

## Infrastructure as Code (Bicep)
- **Language:** Bicep
- **Location:** backend_cloud/iac/main.bicep
- **Azure Resources:** IoT Hub, App Service, SQL Server, SQL Database
- **Resource Group:** rfid-warehouse-rg-no (norwayeast)
- **Configuration:** Parameterized, outputs for CI/CD

## GitHub Workflows (CI/CD)
- **deploy-infra.yml:** Infrastructure deployment
- **deploy.yml:** Application build and deployment
- **Build Backend:** Restore, Build, Test, Publish
- **Build Frontend:** Install, Build (Vite)
- **Deploy:** Publishes to Azure App Service

## Authentication & Security
- **JWT:** 480-minute expiry, dev secret key
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
- **Base URL (Azure):** https://rfid-warehouse-dev-app-26phf7ltazvva.azurewebsites.net
- **Base URL (Local Dev):** http://localhost:5218
- **Auth:** /api/auth/register, /api/auth/login
- **Items:** /api/items
- **Transactions:** /api/transactions
- **Sessions:** /api/sessions
- **SignalR Hub:** /hubs/kiosk

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
5. EF Core migrations applied

## Key Technologies & Versions
- Vue 3.5.25, Vite 7.2.7, Sass 1.95.1
- ASP.NET Core 8.0, EF Core 8.0.11
- Azure: App Service, SQL Database, IoT Hub
- JWT, BCrypt, SignalR
- GitHub Actions

## Final Status
- All infrastructure, backend, frontend, database, and CORS issues resolved
- System is fully functional and ready for production use
