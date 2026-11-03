param(
    [string]$groupName,
    [string[]]$memberIds
)

foreach ($memberId in $memberIds) {
    # Check if the user is already a member of the group
    $isMember = az ad group member check --group $groupName --member-id $memberId --query value 
    if ($isMember -eq 'true') {
        Write-Output "User with objectId: $memberId is already a member of the group '$groupName'."
    } else {
        Write-Host "az ad group member add --group $groupName --member-id $memberId"
        az ad group member add --group $groupName --member-id $memberId
        Write-Output "User with objectId: $memberId added to the group '$groupName'."
    }
}