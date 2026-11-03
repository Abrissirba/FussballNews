param(
    [string]$deploymentPrincipalName,
    [string]$location = "swedencentral"
)

$resourceGroupName = "rg-fussballnews-config"

$deploymentPrincipal = az ad sp list --display-name $deploymentPrincipalName --query "[?appDisplayName=='$deploymentPrincipalName'].{appId:appId, id:id}" | ConvertFrom-Json

$exists = az group exists --name $resourceGroupName --output tsv
if ($exists -eq "false") {
    $subscriptionId = az account show --query id --output tsv
    az group create --name $resourceGroupName --location $location | Out-Null
    az role assignment create --assignee $deploymentPrincipal.id --role Contributor --scope "/subscriptions/${subscriptionId}/resourcegroups/${resourceGroupName}"
} else {
    Write-Host "Resource group '$resourceGroupName' already exists."
}

return [PSCustomObject]@{
    resourceGroupName = $resourceGroupName
}