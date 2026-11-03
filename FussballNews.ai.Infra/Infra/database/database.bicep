param location string
param dbServerName string 
param dbName string
param defaultTagValues object

@description('Create server')
resource dbServer 'Microsoft.Sql/servers@2022-02-01-preview' existing = {
  name: dbServerName
}

resource database 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  parent: dbServer
  name: dbName
  location: location
  sku: {
    capacity: 5
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    recoveryServicesRecoveryPointId: 'F1173C43-91BD-4AAA-973C-54E79E15235B'
    minCapacity: json('0.5')
  }
  tags: defaultTagValues
}


output databaseName string = dbName
