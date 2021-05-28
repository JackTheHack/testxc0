# escape=`
# This Dockerfile is used to generate images for the following roles: mssql

ARG BASE_IMAGE

FROM ${BASE_IMAGE}

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY ./dacpacs ./resources

#COPY ./tools/StartCommerceInit.ps1 ./tools/DeployRoles.ps1 ./tools/CreateExternalDataSources.ps1  ./

# hadolint ignore=DL3025
#ENTRYPOINT .\StartCommerceInit.ps1 -ResourcesDirectory $env:RESOURCES_PATH -SqlServer $env:SQL_SERVER -SqlAdminUser $env:SQL_ADMIN_LOGIN -SqlAdminPassword $env:SQL_ADMIN_PASSWORD -SitecoreAdminPassword $env:sitecore_admin_password -SqlElasticPoolName $env:SQL_ELASTIC_POOL_NAME -CoreDatabaseName $env:SITECORE_CORE_DB -SharedDatabaseName $env:XC_SHARED_DB -ArchiveDatabaseName $env:XC_SHARED_ARCHIVE_DB -MasterkeyPassword $env:XC_DB_Masterkey_Password  `
 #   -DatabaseUsers @( `
    #@{'databaseName' = 'Sitecore.Master'; 'username' = $env:Master_Database_Username; 'password' = $env:Master_Database_Password}, `
  #  )