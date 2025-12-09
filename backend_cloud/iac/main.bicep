// Azure Infrastructure as Code - Bicep template
// IoT Warehouse RFID Project Infrastructure
// Deploys: IoT Hub (Free), App Service (F1), SQL Database (Serverless Free), Storage Account

// Parameters
param location string = 'westeurope'
param environment string = 'dev'
param projectName string = 'rfid-warehouse'
param sqlAdminUsername string
@secure()
param sqlAdminPassword string

// Variables
var uniqueSuffix = uniqueString(resourceGroup().id)
var resourcePrefix = '${projectName}-${environment}'
var iotHubName = '${resourcePrefix}-iothub-${uniqueSuffix}'
var appServiceName = '${resourcePrefix}-app-${uniqueSuffix}'
var appServicePlanName = '${resourcePrefix}-plan'
var sqlServerName = '${resourcePrefix}-sqlserver-${uniqueSuffix}'
var sqlDbName = '${resourcePrefix}-db'
var storageAccountName = '${replace(resourcePrefix, '-', '')}st${uniqueString(resourceGroup().id)}'
var keyVaultName = '${resourcePrefix}-kv-${uniqueSuffix}'
var appInsightsName = '${resourcePrefix}-insights'

// Tags
var commonTags = {
  environment: environment
  project: projectName
  createdBy: 'bicep'
  createdDate: utcNow('u')
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
    features: 'DeviceManagement,RoutingEndpoints'
    routing: {
      endpoints: {
        storageContainers: []
        serviceBusQueues: []
        serviceBusTopics: []
        eventHubs: []
      }
      routes: []
      fallbackRoute: {
        name: '$fallback'
        source: 'DeviceMessages'
        condition: 'true'
        endpointNames: [
          'events'
        ]
        isEnabled: true
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
          connectionString: 'Endpoint=${iotHub.properties.eventHubEndpoints.events.endpoint};SharedAccessKeyName=owner;SharedAccessKey=${listKeys(iotHub.id, '2023-06-30').value[0].primaryKey}'
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

// ===== SQL Database (Serverless Free Tier) =====
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDbName
  location: location
  tags: commonTags
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    capacity: 1
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    autoPauseDelay: 60 // Auto-pause after 1 hour of inactivity
    minCapacity: json('0.5') // 0.5 vCore minimum
    maintenanceConfigurationId: null
  }
}

// ===== Storage Account (for backups/logging) =====
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: commonTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }

  // Create blob container
  resource blobService 'blobServices' = {
    name: 'default'

    resource container 'containers' = {
      name: 'logs'
      properties: {
        publicAccess: 'None'
      }
    }
  }
}

// ===== Application Insights =====
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: commonTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    RetentionInDays: 30
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// ===== Outputs =====
output iotHubHostName string = iotHub.properties.hostName
output iotHubConnectionString string = 'Endpoint=${iotHub.properties.eventHubEndpoints.events.endpoint};SharedAccessKeyName=owner;SharedAccessKey=${listKeys(iotHub.id, '2023-06-30').value[0].primaryKey};EntityPath=${iotHub.properties.eventHubEndpoints.events.path}'
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output sqlServerName string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output storageAccountName string = storageAccount.name
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output resourceGroupName string = resourceGroup().name
