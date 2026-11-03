param(
    [string]$name
)

$spName = "sp-fussballnews-${name}"
$subscriptionId = az account show --query "{subscriptionId:id}" -o tsv

$existingSp = az ad sp list --display-name $spName --query "[?appDisplayName=='$spName'].{appId:appId, id:id}" | ConvertFrom-Json

if ($existingSp -and $existingSp.Length -gt 0) {
    $sp = $existingSp[0]
    Write-Host "Service principal already exists with appId: $($sp.appId)"
    return [PSCustomObject]@{
        appId = $sp.appId
        objectId = $sp.id
        name = $spName
    }
} else {
    $sp = az ad sp create-for-rbac --name $spName | ConvertFrom-Json
    $app = az ad sp list --display-name $spName --query "[?appDisplayName=='$spName'].{appId:appId, id:id}" | ConvertFrom-Json

    Write-Host "Service principal created with appId: " $sp.appId
    Write-Host "Client secret: " $sp.password ". Do not check this into source control!"
    return [PSCustomObject]@{
        appId = $app.appId
        objectId = $app.id
        name = $spName
        clientSecret = $sp.password
    }
}