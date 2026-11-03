param environment string
param location string
param defaultTagValues object 

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-fussballnews-${environment}'
  location: location
  tags: union(defaultTagValues, {})
}

output identityName string = identity.name
output identityId string = identity.id
output identityClientId string = identity.properties.clientId
output identityPrincipalId string = identity.properties.principalId
