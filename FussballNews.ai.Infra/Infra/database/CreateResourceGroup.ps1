param(
    [string]$deploymentPrincipalName,
    [string]$env = "test",
    [string]$location = "swedencentral"
)

$resourceGroupName = "rg-fussballnews-data-${env}"


$deploymentPrincipal = az ad sp list --display-name $deploymentPrincipalName --query "[?appDisplayName=='$deploymentPrincipalName'].{appId:appId, id:id}" | ConvertFrom-Json
$currentUserId = az ad signed-in-user show --query id --output tsv
$exists = az group exists --name $resourceGroupName --output tsv

if ($exists -eq "false") {
    $subscriptionId = az account show --query id --output tsv
    az group create --name $resourceGroupName --location $location | Out-Null
    az role assignment create --assignee $deploymentPrincipal.id --role Contributor --scope "/subscriptions/${subscriptionId}/resourcegroups/${resourceGroupName}"
} else {
    Write-Host "Resource group '$resourceGroupName' already exists."
}

$scriptPath = Join-Path -Path $PSScriptRoot -ChildPath "../entra-id/CreateGroup.ps1"
& $scriptPath `
    -groupName "fussballnews-dbadmin-${env}" `
    -memberIds @($deploymentPrincipal.id, $currentUserId)

return [PSCustomObject]@{
    resourceGroupName = $resourceGroupName
}