param env string
param location string = 'swedencentral' 
@description('The id of the deployment principal.')
param adminGroupId string
param setupCustomDomain bool = false
param customDomains string
param appConfigEndpoint string

var keyVaultName = 'kv-fussballnews-${env}'
var defaultTagValues = {
  env: env
}


@description('The identity that will be used be the app services')
module identity 'identity.bicep' = {
  name: 'identity'
  params: {
    environment: env
    location: location
    defaultTagValues: defaultTagValues
  }
}

@description('Create key vault')
module keyvault 'keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    identityPrincipalId: identity.outputs.identityPrincipalId
    adminGroupId: adminGroupId
    keyVaultName: keyVaultName
    defaultTagValues: defaultTagValues
  }
}

@description('Create blob storage account')
module blobStorage 'storage.bicep' = {
  name: 'blobStorage'
  params: {
    location: location
    env: env
    keyVaultName: keyVaultName
  }
}


@description('Create app insights')
module appinsights 'appinsights.bicep' = if(env != 'localdev') {
  name: 'appinsights'
  params: {
    location: location
    env: env
  }
}

@description('Create app service plan')
module appserviceplan 'appserviceplan.bicep' = if(env != 'localdev') {
  name: 'appserviceplan'
  params: {
    location: location
    env: env
  }
}

@description('Create service bus')
module servicebus 'servicebus.bicep' = {
  name: 'servicebus'
  params: {
    location: location
    env: env
    keyVaultName: keyVaultName
  }
}

@description('Create web app')
module webapp 'webapp.bicep' = if(env != 'localdev') {
  name: 'webapp'
  params: {
    location: location
    env: env
    appInsightsInstrumentationKey: appinsights.outputs.InstrumentationKey
    appInsightsConnectionString: appinsights.outputs.ConnectionString
    serverFarmId: appserviceplan.outputs.serverFarmId
    identityId: identity.outputs.identityId
    identityClientId: identity.outputs.identityClientId
    customDomains: customDomains
    setupCustomDomain: setupCustomDomain
    defaultTagValues: defaultTagValues
    appConfigEndpoint: appConfigEndpoint
  }
}

output identityName string = identity.outputs.identityName
output identityId string = identity.outputs.identityId
output identityPrincipalId string = identity.outputs.identityPrincipalId
output storageAccountName string = blobStorage.outputs.storageAccountName
output backendAzureUrl string = (env != 'localdev') ? 'https://${webapp.outputs.webAzureUrl}' : 'http://localhost:5000'
output backendAppName string = (env != 'localdev') ? webapp.outputs.webAppName : 'localdev'
output keyVaultName string = keyVaultName
