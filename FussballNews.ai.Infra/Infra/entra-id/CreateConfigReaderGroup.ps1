# param(
#     [string]$env,
#     [string[]]$memberIds
# )

$group = & "./CreateGroup.ps1" `
    -groupName "fussballnews-config-reader"

return $group