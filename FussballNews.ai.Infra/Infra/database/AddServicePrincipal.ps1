param(
    [string]$serverName = "fussballnews-db-server-test",
    [string]$databaseName = "fussballnews-db-test",
    [string]$servicePrincipalName = "fussballnews"
)

$accessToken = az account get-access-token --resource https://database.windows.net/ --query accessToken --output tsv

Invoke-Sqlcmd -ServerInstance "${serverName}.database.windows.net" -Database $databaseName -AccessToken $accessToken -Query "
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'${servicePrincipalName}' AND type = 'E')
BEGIN
    CREATE USER [${servicePrincipalName}] FROM EXTERNAL PROVIDER;
END
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = N'${servicePrincipalName}' AND type = 'E')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.database_role_members AS rm JOIN sys.database_principals AS p ON rm.role_principal_id = p.principal_id WHERE p.name = N'db_owner' AND rm.member_principal_id = DATABASE_PRINCIPAL_ID(N'${servicePrincipalName}'))
    BEGIN
        ALTER ROLE db_owner ADD MEMBER [${servicePrincipalName}];
    END
END
"