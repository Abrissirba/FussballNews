param(
    [string]$env,
    [string[]]$memberIds
)

$group = & "./CreateGroup.ps1" `
    -groupName "fussballnews-dbadmin-${env}" `
    -memberIds $memberIds

return $group