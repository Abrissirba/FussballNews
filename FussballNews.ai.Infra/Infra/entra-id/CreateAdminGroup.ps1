param(
    [string[]]$memberIds
)

$group = & "./CreateGroup.ps1" `
    -groupName "fussballnews-admin" `
    -memberIds $memberIds

return $group