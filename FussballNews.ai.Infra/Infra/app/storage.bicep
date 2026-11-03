param env string
param location string
param storageSku string = 'Standard_LRS'
param keyVaultName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: 'bsfussballnews${env}'
  location: location
  sku: {
    name: storageSku
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: true
    accessTier: 'Hot'
  }
}

var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' existing = {
  name: keyVaultName
}

resource storageConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-06-01-preview' = {
  parent: keyVault
  name: 'AzureStorageConnectionString'
  properties: {
    value: storageConnectionString
  }
}

resource articleHtmlContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/article-html'
  properties: {
    publicAccess: 'None'
  }
}


output storageAccountName string = storageAccount.name
output storageConnectionStringSecret string = storageConnectionStringSecret.properties.secretUri
