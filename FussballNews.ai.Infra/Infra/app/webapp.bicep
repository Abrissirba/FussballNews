param env string
param location string
param serverFarmId string
param appInsightsInstrumentationKey string
param appInsightsConnectionString string
param defaultTagValues object
param identityId string
param identityClientId string
param customDomains string
param setupCustomDomain bool
param appConfigEndpoint string

var apiName = 'fussballnews-${env}'

resource webServer 'Microsoft.Web/sites@2023-12-01' = {
  name: apiName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    serverFarmId: serverFarmId
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
    }
    httpsOnly: true
    keyVaultReferenceIdentity: identityId
  }
  tags: defaultTagValues
}

resource webServiceAppSettings 'Microsoft.Web/sites/config@2020-06-01' = {
  parent: webServer
  name: 'appsettings'
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsightsInstrumentationKey
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsightsConnectionString
    ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
    AZURE_CLIENT_ID: identityClientId
    AzureAppConfig__EndpointUrl: appConfigEndpoint
    AzureAppConfig__Label: env
  }
}

resource webServiceLogSettings 'Microsoft.Web/sites/config@2020-06-01' = {
  parent: webServer
  name: 'logs'
  properties: {
    applicationLogs: {
      fileSystem: {
        level: 'Warning'
      }
    }
    httpLogs: {
      fileSystem: {
        retentionInMb: 40
        enabled: true
      }
    }
    failedRequestsTracing: {
      enabled: true
    }
    detailedErrorMessages: {
      enabled: true
    }
  }
}

var hostnames = split(customDomains, ';')

@batchSize(1)
resource customDomainsWithoutSsl 'Microsoft.Web/sites/hostNameBindings@2022-09-01' =  [for hostName in hostnames : if(setupCustomDomain == true) {
  name: hostName
  kind: 'string'
  parent: webServer
  properties: {
    azureResourceName: apiName
    azureResourceType: 'Website'
    customHostNameDnsRecordType: 'CName'
    siteName: '${apiName}.azurewebsites.net'
    sslState: 'Disabled'
    hostNameType: 'Verified'
  }
}]

module certificateBindings './bindCertificateToHostname.bicep' = if(setupCustomDomain == true) {
  name: '${deployment().name}-ssl'
  params: {
    appServicePlanResourceId: serverFarmId
    customHostnames: hostnames
    location: location
    webAppName: apiName
  }
  dependsOn: customDomainsWithoutSsl
}

output webAzureUrl string = '${webServer.name}.azurewebsites.net'
output webAppName string = webServer.name
