@description('The environment to create for. Can be whatever you like.')
param env string

@description('The geographical location for resource group and all resources.')
param location string


var appServicePlanName = 'appserviceplan-fussballnews-${env}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B2'
    capacity: 2
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

output serverFarmId string = appServicePlan.id
