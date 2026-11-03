@description('The environment to create for. Can be whatever you like.')
param env string

@description('The geographical location for resource group and all resources.')
param location string

resource workspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: 'loganalytics-workspace-fussballnews-${env}'
  location: location
  properties: {
    sku: {
      name: 'Standalone'
    }
    retentionInDays: 120
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appInsights-fussballnews-${env}'
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'Web'
    Flow_Type: 'Bluefield'
    WorkspaceResourceId: workspace.id
  }
}

output InstrumentationKey string = appInsights.properties.InstrumentationKey
output ConnectionString string = appInsights.properties.ConnectionString
