// Azure Infrastructure as Code - Bicep template
// IoT Warehouse RFID Project Infrastructure
// Deploys: IoT Hub (Free), App Service (F1), SQL Database (Basic - Free with Azure for Students)

// Parameters
param location string = 'westeurope'
param environment string = 'dev'
param projectName string = 'rfid-warehouse'
param sqlAdminUsername string
@secure()
param sqlAdminPassword string
// Capture creation timestamp once (allowed in parameter default)
param createdDate string = utcNow('u')

// Variables
var uniqueSuffix = uniqueString(resourceGroup().id)
var resourcePrefix = '${projectName}-${environment}'
var iotHubName = '${resourcePrefix}-iothub-${uniqueSuffix}'
var appServiceName = '${resourcePrefix}-app-${uniqueSuffix}'
var appServicePlanName = '${resourcePrefix}-plan'
var sqlServerName = '${resourcePrefix}-sqlserver-${uniqueSuffix}'
var sqlDbName = '${resourcePrefix}-db'

// Tags
var commonTags = {
  environment: environment
  project: projectName
  createdBy: 'bicep'
  createdDate: createdDate
}

// ===== IoT Hub (Free Tier - 8000 messages/day) =====
resource iotHub 'Microsoft.Devices/IotHubs@2023-06-30' = {
  name: iotHubName
  location: location
  tags: commonTags
  sku: {
    name: 'F1'
    capacity: 1
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    eventHubEndpoints: {
      events: {
        retentionTimeInDays: 1
      }
    }
  }
}

// ===== App Service Plan (Linux F1 - Free Tier) =====
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: commonTags
  kind: 'Linux'
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  properties: {
    reserved: true
  }
}

// ===== App Service (Linux) =====
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: appServiceName
  location: location
  tags: commonTags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: false
      http20Enabled: true
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
        {
          name: 'ASPNETCORE_URLS'
          value: 'http://+:80'
        }
      ]
      connectionStrings: [
        {
          name: 'WarehouseDb'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDbName};Persist Security Info=False;User ID=${sqlAdminUsername};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
          type: 'SQLAzure'
        }
        {
          name: 'IoTHub'
          connectionString: 'HostName=${iotHub.properties.hostName};SharedAccessKeyName=owner;SharedAccessKey=${iotHub.listkeys().value[0].primaryKey}'
          type: 'Custom'
        }
      ]
    }
    httpsOnly: true
  }
}

// ===== SQL Server =====
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  tags: commonTags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }

  // Firewall rule for Azure services
  resource firewallRule 'firewallRules@2023-08-01-preview' = {
    name: 'AllowAllAzureServices'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }
}

// ===== SQL Database (Basic Tier - Free with Azure for Students) =====
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDbName
  location: location
  tags: commonTags
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2 GB
    zoneRedundant: false
  }
}



// ===== Outputs =====
output iotHubHostName string = iotHub.properties.hostName
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output sqlServerName string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output resourceGroupName string = resourceGroup().name
