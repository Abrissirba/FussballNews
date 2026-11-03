param env string
param location string
param dbAdminGroupId string
param dbAdminGroupName string

var serverName = 'sql-fussballnews-${env}'
var dbName = 'db-fussballnews-${env}'

var defaultTagValues = {
  env: env
}

module server 'server.bicep' = {
  name: 'server'
  params: {
    location: location
    dbServerName: serverName
    defaultTagValues: defaultTagValues
    dbAdminGroupId: dbAdminGroupId
    dbAdminGroupName: dbAdminGroupName
  }
}

// module database 'database.bicep' = {
//   name: 'database'
//   params: {
//     location: location
//     dbServerName: serverName
//     dbName: dbName
//     defaultTagValues: defaultTagValues
//   }
//   dependsOn: [
//     server
//   ]
// }

output dbServerName string = server.outputs.serverName
// output databaseName string = database.outputs.databaseName


