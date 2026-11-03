param(
    [string]$configStoreName = "appconfig-fussballnews",
    [string]$location = "swedencentral",
    [string]$keyVaultName = "kv-fussballnews-config",
    [string]$resourceGroupName = "rg-fussballnews-config"
)

$deploymentName = "fussballnews_config_infra"

$Stopwatch = [System.Diagnostics.Stopwatch]::new()
$Stopwatch.Start()
Write-Output "Deploying environment $deploymentName ..."

$result = az deployment group create `
    --name ${deploymentName} `
    --resource-group $resourceGroupName `
    --template-file main.bicep `
    -p configStoreName=$configStoreName `
    -p location=$location `
    -p keyVaultName=$keyVaultName 

$jsonoutput = $result | ConvertFrom-Json

Write-Output $jsonoutput

$Stopwatch.Stop()
Write-Output $Stopwatch.Elapsed