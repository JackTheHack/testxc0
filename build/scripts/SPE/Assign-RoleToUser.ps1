param(
    [string]$instanceUrl = "project.dev.local",
    [string]$adminUsername,
    [string]$adminPassword,
    [string]$username,
    [string]$role
)

$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username $adminUsername -Password $adminPassword -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    Add-RoleMember -Identity $using:role -Members $using:username
}