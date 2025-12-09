# Azure Deployment Guide

Complete step-by-step guide to deploy the RFID Warehouse system to Azure.

## Prerequisites

- Azure Subscription (free account works)
- Azure CLI installed
- GitHub account with the repository
- Git installed locally

## Step 1: Create Azure Service Principal

For GitHub Actions to deploy automatically:

```powershell
# Login to Azure
az login

# Create service principal
az ad sp create-for-rbac `
  --name "rfid-warehouse-sp" `
  --role contributor `
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID

# Copy the output JSON (you'll need this next)
```

## Step 2: Create Azure Resource Group

```powershell
# Create resource group
az group create `
  --name rfid-warehouse-rg `
  --location westeurope
```

## Step 3: Deploy Infrastructure with Bicep

```powershell
# Deploy all Azure resources
az deployment group create `
  --resource-group rfid-warehouse-rg `
  --template-file backend_cloud/iac/main.bicep `
  --parameters @backend_cloud/iac/parameters.json `
  --parameters sqlAdminPassword="YourSecurePassword123!" `
  --parameters location=westeurope

# Note the output values (App Service URL, SQL Server name, etc.)
```

## Step 4: Configure GitHub Secrets

Add these secrets to your GitHub repository (Settings > Secrets and variables > Actions):

### Required Secrets

1. **AZURE_CREDENTIALS**
   - Paste the entire JSON from the service principal creation above
   - Used by GitHub Actions to authenticate with Azure

2. **AZURE_PUBLISH_PROFILE**
   - Go to Azure Portal > App Service > Download publish profile
   - Content of the XML file

3. **AZURE_APP_NAME**
   - From Bicep output: App Service name (e.g., `rfid-warehouse-app-abc123`)

4. **AZURE_RESOURCE_GROUP**
   - The resource group name: `rfid-warehouse-rg`

5. **DB_CONNECTION_STRING**
   - Format: `Server=tcp:YOUR_SQL_SERVER.database.windows.net,1433;Initial Catalog=rfid-warehouse-db;Persist Security Info=False;User ID=sqladmin;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
   - Replace placeholders from Bicep outputs

6. **SQL_ADMIN_PASSWORD**
   - The password you used during Bicep deployment

## Step 5: Register IoT Hub Device

Create a device in Azure IoT Hub for the Raspberry Pi:

```powershell
# Get IoT Hub name from Bicep output
$IOT_HUB_NAME = "rfid-warehouse-iothub-XXX"
$DEVICE_ID = "rpi-scanner-01"

# Create device
az iot hub device-identity create `
  --hub-name $IOT_HUB_NAME `
  --device-id $DEVICE_ID

# Get connection string for Raspberry Pi
az iot hub device-identity connection-string show `
  --hub-name $IOT_HUB_NAME `
  --device-id $DEVICE_ID
```

Copy the connection string for use in Raspberry Pi setup.

## Step 6: Configure Raspberry Pi

On your Raspberry Pi:

```bash
# Clone/download project
cd ~/rfid-scanner

# Create .env file
cp edge_rpi/.env.example edge_rpi/.env

# Edit .env with your values
nano edge_rpi/.env

# Install dependencies
python3 -m venv venv
source venv/bin/activate
pip install -r edge_rpi/requirements.txt

# Test the connection
python edge_rpi/rfid_scanner.py
```

## Step 7: Trigger GitHub Actions Deployment

Push code to main branch to trigger automatic deployment:

```powershell
git add .
git commit -m "Deploy to Azure"
git push origin main
```

Watch the workflow at GitHub > Actions > Build and Deploy to Azure

## Step 8: Verify Deployment

### Check App Service

```powershell
# Get App Service URL
az webapp show --resource-group rfid-warehouse-rg --name YOUR_APP_NAME --query defaultHostName -o tsv

# Open in browser
https://YOUR_APP_NAME.azurewebsites.net
```

### Apply Database Migrations

If migrations didn't run automatically:

```powershell
az webapp up `
  --resource-group rfid-warehouse-rg `
  --name YOUR_APP_NAME
```

Then access `/admin/health` to verify database connection.

### Monitor Application

```powershell
# Stream app logs
az webapp log tail --resource-group rfid-warehouse-rg --name YOUR_APP_NAME
```

## Step 9: Configure Custom Domain (Optional)

```powershell
# Add custom domain
az webapp config hostname add `
  --resource-group rfid-warehouse-rg `
  --webapp-name YOUR_APP_NAME `
  --hostname yourdomain.com

# Configure SSL certificate (Let's Encrypt free option)
az webapp config ssl upload-certificate `
  --resource-group rfid-warehouse-rg `
  --name YOUR_APP_NAME `
  --certificate-file mycert.pfx `
  --certificate-password password
```

## Monitoring & Cost Optimization

### View Costs

```powershell
# Estimate monthly costs
az billing usage list --query "value[?contains(name, 'rfid')].{Name:name, UsageQuantity:usageQuantity, MeterCategory:meterCategory}"
```

### Enable Auto-shutdown for VM (if using)

```powershell
az vm auto-shutdown-time create \
  --resource-group rfid-warehouse-rg \
  --vm-name YOUR_VM_NAME \
  --time 20:00
```

### Monitor SignalR Connections

In Azure Portal:
- App Service > Monitoring > Metrics
- Select "Connections" metric
- Set time range to last 1 hour

## Troubleshooting Deployment

### "Authentication failed - check AZURE_CREDENTIALS"

Verify the GitHub secret contains complete service principal JSON.

### "App Service deployment failed"

Check the build logs:
```powershell
az webapp deployment source config-zip \
  --resource-group rfid-warehouse-rg \
  --name YOUR_APP_NAME \
  --src deployment.zip
```

### "Database migration failed"

Check connection string in app settings:
```powershell
az webapp config connection-string list \
  --resource-group rfid-warehouse-rg \
  --name YOUR_APP_NAME
```

### "SignalR connection refused"

Ensure `/hubs/kiosk` endpoint is accessible:
```bash
curl https://YOUR_APP_NAME.azurewebsites.net/hubs/kiosk
# Should return 426 (Upgrade Required) for non-WebSocket
```

## Updating Deployment

After code changes:

```powershell
# Update infrastructure if needed
az deployment group create \
  --resource-group rfid-warehouse-rg \
  --template-file backend_cloud/iac/main.bicep \
  --parameters @backend_cloud/iac/parameters.json \
  --parameters sqlAdminPassword="UpdatedPassword123!"

# Redeploy application via git push (triggers GitHub Actions)
git push origin main
```

## Rollback Deployment

If deployment fails:

```powershell
# View deployment history
az deployment group list \
  --resource-group rfid-warehouse-rg \
  --query "[].{Name:name, State:properties.provisioningState}"

# Rollback to previous deployment
az deployment group what-if \
  --resource-group rfid-warehouse-rg \
  --template-file backend_cloud/iac/main.bicep
```

## Cost Saving Tips

1. **SQL Database Auto-pause**: Already enabled (pauses after 1 hour)
2. **App Service F1**: 60 minutes compute time per day limit
3. **IoT Hub Free Tier**: 8000 messages/day
4. **Storage**: Use Blob lifecycle to archive old logs
5. **Monitor**: Set up billing alerts (> $10/day)

## Security Checklist

- [ ] Change default SQL admin password
- [ ] Enable Azure Defender for App Service
- [ ] Configure firewall rules for SQL Server
- [ ] Enable HTTPS only on App Service
- [ ] Rotate connection strings monthly
- [ ] Review access logs weekly
- [ ] Update dependencies monthly

## Support

For deployment issues:
1. Check Azure Activity Log for error details
2. Review Application Insights metrics
3. Check GitHub Actions workflow logs
4. Consult [Azure Bicep Documentation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
