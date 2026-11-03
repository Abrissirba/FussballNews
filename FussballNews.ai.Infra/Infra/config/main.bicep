param configStoreName string = 'appconfig-fussballnews'
param location string = 'swedencentral' 
@description('The id of the deployment principal.')
param keyVaultName string = 'kv-fussballnews-config'

@description('Create key vault')
module keyvault 'keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    keyVaultName: keyVaultName
  }
}

@description('Create app config')
module appconfig 'appconfig.bicep' = {
  name: 'appconfig'
  params: {
    location: location
    configStoreName: configStoreName
  }
}
