# Setup
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$timeFormat = "HH:mm:ss:fff"

# print start message
Write-Host ("$(Get-Date -Format $timeFormat): xConnect Worker ENTRYPOINT, starting...")

# Check to see if we should start the Watch-Directory.ps1 script
$watchDirectoryJobName = "Watch-Directory.ps1"
$useWatchDirectory = $null -ne $WatchDirectoryParameters -bor (Test-Path -Path "C:\deploy" -PathType "Container") -eq $true

if ($useWatchDirectory)
{
    # Setup default parameters if none is supplied
    if ($null -eq $WatchDirectoryParameters)
    {
        $WatchDirectoryParameters = @{ Path = "C:\deploy"; Destination = "C:\service"; }
    }

    Write-Host "$(Get-Date -Format $timeFormat): xConnect Worker ENTRYPOINT: '$watchDirectoryJobName' validating..."

    # First a trial-run to catch any parameter validation / setup errors
    $WatchDirectoryParameters["WhatIf"] = $true
    & "C:\tools\scripts\Watch-Directory.ps1" @WatchDirectoryParameters
    $WatchDirectoryParameters["WhatIf"] = $false
    
    Write-Host "$(Get-Date -Format $timeFormat): xConnect Worker ENTRYPOINT: '$watchDirectoryJobName' starting..."

    # Start Watch-Directory.ps1 in background
    Start-Job -Name $watchDirectoryJobName -ArgumentList $WatchDirectoryParameters -ScriptBlock {
        param([hashtable]$params)

        & "C:\tools\scripts\Watch-Directory.ps1" @params

    } | Out-Null

    Write-Host "$(Get-Date -Format $timeFormat): xConnect Worker ENTRYPOINT: '$watchDirectoryJobName' started."
}
else
{
    Write-Host "$(Get-Date -Format $timeFormat): xConnect Worker ENTRYPOINT: Skipping start of '$watchDirectoryJobName'. To enable you should mount a directory into 'C:\deploy'."
}

# start xConnect worker process in background, kill foreground process if it fails
Start-Job -Name $env:WORKER_EXECUTABLE_NAME_ENV {
    try
    {
        & "C:\service\$env:WORKER_EXECUTABLE_NAME_ENV" 
    }
    finally
    {
        Get-Process -Name "filebeat" | Stop-Process -Force
    }
} | Out-Null

# wait for the xConnect Worker process is running
while ($true)
{
    $processName = $env:WORKER_EXECUTABLE_NAME_ENV
    if($processName.Contains(".exe")){
        $processName = $processName.Replace(".exe","")
    }

    Write-Host "$(Get-Date -Format $timeFormat): Waiting for process '$processName' to start..."

    $running = [array](Get-Process -Name $processName -ErrorAction "SilentlyContinue").Length -eq 1

    if ($running)
    {
        Write-Host "$(Get-Date -Format $timeFormat): Process '$processName' started..."

        break;
    }

    Start-Sleep -Milliseconds 500
}

# update configuration files
$serilogFilePath = "C:/service/app_data/config/sitecore/coreservices/sc.serilog.xml"
if($useWatchDirectory){
    $serilogFilePath = "C:/deploy/app_data/config/sitecore/coreservices/sc.serilog.xml"
}
& "C:\tools\scripts\Set-Config.ps1" `
    -AppInsightsRoleName $env:SITECORE_APPSETTINGS_AppInsightsRoleName `
    -SerilogFilePath $serilogFilePath

# print ready message
Write-Host ("$(Get-Date -Format $timeFormat): xConnect Worker ready!")

# start filebeat.exe in foreground
& "C:\tools\bin\filebeat\filebeat.exe" -c (Join-Path $PSScriptRoot "\filebeat.yml")