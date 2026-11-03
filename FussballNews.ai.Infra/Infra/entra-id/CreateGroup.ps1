#.\create-group.ps1 -groupName "MyNewGroup" -memberIds @("memberId1", "memberId2", "memberId3")
# az ad signed-in-user show --query id --output tsv

param(
    [string]$groupName,
    [string[]]$memberIds
)

# Create the Azure AD group
$groupId = az ad group create --display-name $groupName `
                            --mail-nickname $groupName `
                            --force false `
                            --query id `
                            --output tsv


if ($null -ne $memberIds -and $memberIds.Length -gt 0) {
    Write-Host "Adding members"

    foreach ($memberId in $memberIds) {
        # Check if the user is already a member of the group
        Write-Host "Add member " $memberId "to group " $groupName
        $isMember = az ad group member check --group $groupName --member-id $memberId --query value 
        if ($isMember -eq 'true') {
            Write-Output "User with objectId: $memberId is already a member of the group '$groupName'."
        } else {
            Write-Host "az ad group member add --group $groupName --member-id $memberId"
            az ad group member add --group $groupName --member-id $memberId
            Write-Output "User with objectId: $memberId added to the group '$groupName'."
        }
    }
}

$outputObject = [PSCustomObject]@{
    groupId = $groupId
}

Write-Host "Created group with id: " $groupId

return $outputObject