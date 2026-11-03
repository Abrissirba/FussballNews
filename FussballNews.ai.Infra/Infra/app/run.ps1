param(
    [string]$env = "localdev",
    [string]$location = "swedencentral",
    [string]$setupCustomDomain = "false",
    [string]$customDomains = "fussballnews.ai",
    [string]$appConfigName = "appconfig-fussballnews",
    [string]$appConfigResourceGroup = "rg-fussballnews-config",
    [string]$dbServerName = "sql-fussballnews-$env.database.windows.net",
    [string]$dbName = "db-fussballnews-${env}",
    [string]$adminGroupName = "fussballnews-admin"
)

if ($env -eq "localdev") {
    $dbServerName = "."
}

$deploymentName = "fussballnews_app_${env}_infra"

$Stopwatch = [System.Diagnostics.Stopwatch]::new()
$Stopwatch.Start()
Write-Output "Deploying environment $deploymentName ..."

$appConfigEndpoint = az appconfig show --name $appConfigName --resource-group $appConfigResourceGroup --query endpoint --output tsv
$adminGroupId = az ad group show --group $adminGroupName --query id --output tsv
$resourceGroup = "rg-fussballnews-app-$env"

$result = az deployment group create `
     --name ${deploymentName} `
     --resource-group $resourceGroup `
     --template-file main.bicep `
     -p location=$location `
     -p env=$env `
     -p adminGroupId=$adminGroupId `
     -p setupCustomDomain=$setupCustomDomain `
     -p customDomains=$customDomains `
     -p appConfigEndpoint=$appConfigEndpoint `
     --query "properties.outputs"
     | ConvertFrom-Json

$identityName = $result.identityName.value
$backendAzureUrl = $result.backendAzureUrl.value
$keyVaultName = $result.keyVaultName.value
$identityPrincipalId = $result.identityPrincipalId.value

if ($dbServerName.Contains("database.windows.net")) {
    $scriptPath = Join-Path -Path $PSScriptRoot -ChildPath "../database/AddManagedIdentityUser.ps1"
    & $scriptPath `
        -serverName $dbServerName `
        -databaseName $dbName `
        -managedIdentityName $identityName
}

Write-Host "Assigning roles to the identity"
$appConfigResourceId = az appconfig list --resource-group $appConfigResourceGroup --query "[0].id" --output tsv
az role assignment create --assignee $identityPrincipalId --role "App Configuration Data Reader" --scope $appConfigResourceId | Out-Null

$appConfigKeyVaultResourceId = az keyvault list --resource-group $appConfigResourceGroup --query "[0].id" --output tsv
az role assignment create --assignee $identityPrincipalId --role "Key Vault Secrets User" --scope $appConfigKeyVaultResourceId | Out-Null
az role assignment create --assignee $identityPrincipalId --role "Key Vault Reader" --scope $appConfigKeyVaultResourceId | Out-Null

Write-Host "Setting app configuration values"
$connectionString = "Server=$dbServerName;Initial Catalog=$dbName;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication='Active Directory Default'"

az appconfig kv set --name $appConfigName --key "DatabaseConnectionString" --value $connectionString --label $env --yes | Out-Null
az appconfig kv set --name $appConfigName --key "ApiBaseUrl" --value $backendAzureUrl --label $env --yes | Out-Null
Write-Host "Setting app configuration storage key vault reference"
az appconfig kv set-keyvault --name $appConfigName --key "AzureStorageConnectionString" --label $env --secret-identifier "https://${keyVaultName}.vault.azure.net/secrets/AzureStorageConnectionString" --yes
az appconfig kv set-keyvault --name $appConfigName --key "ServiceBusConnectionString" --label $env --secret-identifier "https://${keyVaultName}.vault.azure.net/secrets/ServiceBusConnectionString" --yes
$Stopwatch.Stop()
Write-Output $Stopwatch.Elapsed