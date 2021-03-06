param(
 [string]$instanceUrl = "project.dev.local",
 [string]$hostname,
 [string]$sxaSiteName = "Project",
 [string]$adminUsername,
 [string]$adminPassword)
$ErrorActionPreference = 'Stop'
Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)
$session = New-ScriptSession -Username "$adminUsername" -Password "$adminPassword" -ConnectionUri $("https://" + $instanceUrl)
Invoke-RemoteScript -Session $session -ScriptBlock { 
 (Get-Item -Path ("master:/content/Sites/{0}/Settings/Site Grouping/{0}" -f $using:sxaSiteName)).HostName = ("{0}" -f $using:hostname)
}