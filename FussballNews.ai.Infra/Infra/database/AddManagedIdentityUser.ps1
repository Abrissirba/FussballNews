param(
    [string]$serverName = "fussballnews-db-server-test",
    [string]$databaseName = "fussballnews-db-test",
    [string]$managedIdentityName = "id-fussballnews"
)

Write-Host "Adding managed identity user to database..." $serverName $databaseName

$accessToken = az account get-access-token --resource https://database.windows.net/ --query accessToken --output tsv
Write-Host "Access token obtained"
Invoke-Sqlcmd -ServerInstance "${serverName}" -Database $databaseName -AccessToken $accessToken -Query "
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'${managedIdentityName}' AND type = 'E')
BEGIN
    CREATE USER [${managedIdentityName}] FROM EXTERNAL PROVIDER;
END
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = N'${managedIdentityName}' AND type = 'E')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.database_role_members AS rm JOIN sys.database_principals AS p ON rm.role_principal_id = p.principal_id WHERE p.name = N'db_datareader' AND rm.member_principal_id = DATABASE_PRINCIPAL_ID(N'${managedIdentityName}'))
    BEGIN
        ALTER ROLE db_datareader ADD MEMBER [${managedIdentityName}];
    END
    IF NOT EXISTS (SELECT * FROM sys.database_role_members AS rm JOIN sys.database_principals AS p ON rm.role_principal_id = p.principal_id WHERE p.name = N'db_datawriter' AND rm.member_principal_id = DATABASE_PRINCIPAL_ID(N'${managedIdentityName}'))
    BEGIN
        ALTER ROLE db_datawriter ADD MEMBER [${managedIdentityName}];
    END
    IF NOT EXISTS (SELECT * FROM sys.database_role_members AS rm JOIN sys.database_principals AS p ON rm.role_principal_id = p.principal_id WHERE p.name = N'db_ddladmin' AND rm.member_principal_id = DATABASE_PRINCIPAL_ID(N'${managedIdentityName}'))
    BEGIN
        ALTER ROLE db_ddladmin ADD MEMBER [${managedIdentityName}];
    END
END
" -QueryTimeout 30 -ConnectionTimeout 30

Write-Host "Managed identity user added to database" $serverName $databaseName