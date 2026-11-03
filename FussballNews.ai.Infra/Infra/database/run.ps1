param(
    [string]$env = "test",
    [string]$location = "swedencentral",
    [string]$dbAdminGroupName = "fussballnews-dbadmin-${env}"
)

$deploymentName = "fussballnews_data_${env}_infra"
$resourceGroup = "rg-fussballnews-data-${env}"
$Stopwatch = [System.Diagnostics.Stopwatch]::new()
$Stopwatch.Start()
Write-Output "Deploying environment $deploymentName ..."

$dbGroupAdminId = az ad group show --group $dbAdminGroupName --query id --output tsv

Write-Host $dbGroupAdminId

$result = az deployment group create `
    --name ${deploymentName} `
    --resource-group "rg-fussballnews-data-${env}" `
    --template-file main.bicep `
    -p env=$env `
    -p location=$location `
    -p dbAdminGroupId=$dbGroupAdminId `
    -p dbAdminGroupName=$dbAdminGroupName `
    --query "properties.outputs"
    | ConvertFrom-Json
    
    
    
$dbServerName = $result.dbServerName.value
# $databaseName = $result.databaseName.value

$myIp = (Invoke-WebRequest -uri "http://ifconfig.me/ip").Content

az sql server firewall-rule create -g $resourceGroup -n SetUp `
    -s $dbServerName `
    --start-ip-address $myIp `
    --end-ip-address $myIp `
    | ConvertFrom-Json

$Stopwatch.Stop()
Write-Output $Stopwatch.Elapsed