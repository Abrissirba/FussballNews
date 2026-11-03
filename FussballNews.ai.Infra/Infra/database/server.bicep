param location string
param dbServerName string 
param defaultTagValues object
param dbAdminGroupId string
param dbAdminGroupName string

@description('Create server')
resource dbServerName_resource 'Microsoft.Sql/servers@2022-02-01-preview' = {
  name: dbServerName
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: dbAdminGroupName
      sid: dbAdminGroupId
      tenantId: tenant().tenantId
      azureADOnlyAuthentication: true
    }
    version: '12.0'
  }
  tags: defaultTagValues
}

resource dbServerName_AllowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallrules@2021-02-01-preview' = {
  parent: dbServerName_resource
  name: 'AllowAllWindowsAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

output serverName string = dbServerName


