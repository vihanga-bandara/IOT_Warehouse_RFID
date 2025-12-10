// Azure Infrastructure as Code - Bicep template
// IoT Warehouse RFID Project Infrastructure
// Deploys: IoT Hub (Free), App Service (F1)

// Parameters
param location string = 'westeurope'
param environment string = 'dev'
param projectName string = 'rfid-warehouse'
// Capture creation timestamp once (allowed in parameter default)
param createdDate string = utcNow('u')

// Variables
var uniqueSuffix = uniqueString(resourceGroup().id)
var resourcePrefix = '${projectName}-${environment}'
var iotHubName = '${resourcePrefix}-iothub-${uniqueSuffix}'
var appServiceName = '${resourcePrefix}-app-${uniqueSuffix}'
var appServicePlanName = '${resourcePrefix}-plan'

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
        partitionCount: 2
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
          name: 'IoTHub'
          connectionString: 'HostName=${iotHub.properties.hostName};SharedAccessKeyName=owner;SharedAccessKey=${iotHub.listkeys().value[0].primaryKey}'
          type: 'Custom'
        }
      ]
    }
    httpsOnly: true
  }
}

// ===== Outputs =====
output iotHubHostName string = iotHub.properties.hostName
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output resourceGroupName string = resourceGroup().name
