# RFID Warehouse Tool Management System

RFID-based warehouse tool tracking with a Vue 3 web UI, an ASP.NET Core 8 API, and Azure IoT Hub + SignalR for real-time kiosk updates.

## What’s in this repo
- **frontend_web/**: Vue 3 SPA (Admin + User + Kiosk)
- **backend_cloud/api/**: ASP.NET Core Web API + SignalR hub + EF Core
- **backend_cloud/iac/**: Azure Bicep (IoT Hub, App Service, Azure SQL)
- **edge_rpi/**: Raspberry Pi RFID scanner client

## Run locally

### Prerequisites
- .NET SDK 8.x
- Node.js 20+
- SQL Server (LocalDB or a real SQL Server)

### Backend (API)
```bash
cd backend_cloud/api
dotnet restore
dotnet run
```
- API: `http://localhost:5218`
- Swagger: `http://localhost:5218/swagger`

### Frontend (Vue)
```bash
cd frontend_web
npm install
npm run dev
```
- UI: Vite default is `http://localhost:5173` (or the next free port)

### Local configuration notes
- Frontend calls the API via `VITE_API_URL` (see [frontend_web/.env](frontend_web/.env)). Default: `http://localhost:5218/api`.
- Backend expects these config keys:
  - `ConnectionStrings:DefaultConnection`
  - `Jwt:SecretKey`
  - `IoTHub:EventHubConnectionString` (optional; if missing, the IoT listener won’t start)

## Login behavior (important)
- **Admin users** can log in without a scanner.
- **Non-admin users** must provide a **scanner name** at login (they go directly to the kiosk flow).

## Deploy to Azure (current repo setup)
This repo uses GitHub Actions with Azure OIDC:
- [deploy-infra.yml](.github/workflows/deploy-infra.yml): deploys Bicep resources
- [deploy.yml](.github/workflows/deploy.yml): builds backend+frontend, copies the built SPA into the API `wwwroot/`, then deploys one App Service

### Required GitHub repository secrets
- `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` (OIDC)
- `SQL_ADMIN_USERNAME`, `SQL_ADMIN_PASSWORD` (used during deployment)
- `JWT_SECRET_KEY` (app setting)

For more detailed deployment notes, see [DEPLOYMENT_SETUP.md](DEPLOYMENT_SETUP.md).

## Documentation
- Project overview / details: [specs/knowledge-base.md](specs/knowledge-base.md)
- Raspberry Pi setup: [edge_rpi/README_RPI.md](edge_rpi/README_RPI.md)
