param keyVaultName string
param adminGroupId string
param location string = resourceGroup().location
param defaultTagValues object
param identityPrincipalId string 

@description('Create keyvault')
resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: keyVaultName
  location: location
  tags: defaultTagValues
  properties: {
    enabledForDeployment: true
    tenantId: tenant().tenantId
    enableSoftDelete: false
    enableRbacAuthorization: false
    accessPolicies: [
    ]
    sku: {
      name: 'standard'
      family: 'A'
    }
  }
}

var accessPolices = [
  {
    objectId: adminGroupId
    secrets: [
      'all'
    ]
    keys: [
      'all'
    ]
    certificates: [
      'all' 
    ]
  }
  {
    objectId: identityPrincipalId
    secrets: [
      'get'
      'list'
    ]
    keys: []
    certificates: []
  }
  
]


@description('Add access policy for the SettingsUpdater app registration and listed users.')
resource keyVaultPolicies 'Microsoft.KeyVault/vaults/accessPolicies@2021-10-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    // Loop over configured access policies.
    accessPolicies: [for accessPolicy in accessPolices: {
      objectId: accessPolicy.objectId
      permissions: {
        secrets: accessPolicy.secrets
        keys: accessPolicy.keys
        certificates: accessPolicy.certificates
      }
      tenantId: subscription().tenantId
    }]
  }
}

// To purge a soft-deleted key vault (change name to correct):
//   `az keyvault purge --name kv-lbs-[lbsEnvironment]`
// This can be handy if the resource group is deleted. Then the key vault is just soft-deleted. And this script will fail.

