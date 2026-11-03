@description('Specifies the name of the App Configuration store.')
param configStoreName string = 'appconfig-fussballnews'

@description('The geographical location for resource group and all resources.')
param location string = 'swedencentral' 

resource configStore 'Microsoft.AppConfiguration/configurationStores@2021-10-01-preview' = {
  name: configStoreName
  location: location
  sku: {
    name: 'free'
  }
}
