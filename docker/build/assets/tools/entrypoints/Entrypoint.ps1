# Setup
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$timeFormat = "HH:mm:ss:fff"

Import-Module WebAdministration
function Wait-WebItemState
{
    param(
        [ValidateNotNullOrEmpty()]
        [string]$IISPath
        ,
        [ValidateSet("Started", "Stopped")]
        [string]$State
    )

    while ($true)
    {
        Write-Host "$(Get-Date -Format $timeFormat): Waiting on item '$IISPath' state to be '$State'..."

        try
        {
            $item = Get-Item -Path $IISPath

            if ($null -ne $item -and $item.State -ne $State)
            {
                if ($State -eq "Started")
                {
                    $item = Start-WebItem -PSPath $IISPath -Passthru -ErrorAction "SilentlyContinue"
                }
                elseif ($State -eq "Stopped")
                {
                    $item = Stop-WebItem -PSPath $IISPath -Passthru -ErrorAction "SilentlyContinue"
                }
            }
        }
        catch
        {
            $item = $null
        }

        if ($null -ne $item -and $item.State -eq $State)
        {
            Write-Host "$(Get-Date -Format $timeFormat): Waiting on item '$IISPath' completed."

            break
        }

        Start-Sleep -Milliseconds 500
    }
}

# print start message
Write-Host ("$(Get-Date -Format $timeFormat): xConnect ENTRYPOINT, starting...")

# Check to see if we should start the Watch-Directory.ps1 script
$watchDirectoryJobName = "Watch-Directory.ps1"
$useWatchDirectory = $null -ne $WatchDirectoryParameters -bor (Test-Path -Path "C:\deploy" -PathType "Container") -eq $true

if ($useWatchDirectory)
{
    # Setup default parameters if none is supplied
    if ($null -eq $WatchDirectoryParameters)
    {
        $WatchDirectoryParameters = @{ Path = "C:\deploy"; Destination = "C:\inetpub\wwwroot"; }
    }

    Write-Host "$(Get-Date -Format $timeFormat): xConnect ENTRYPOINT: '$watchDirectoryJobName' validating..."

    # First a trial-run to catch any parameter validation / setup errors
    $WatchDirectoryParameters["WhatIf"] = $true
    & "C:\tools\scripts\Watch-Directory.ps1" @WatchDirectoryParameters
    $WatchDirectoryParameters["WhatIf"] = $false
    
    Write-Host "$(Get-Date -Format $timeFormat): xConnect ENTRYPOINT: '$watchDirectoryJobName' starting..."

    # Start Watch-Directory.ps1 in background
    Start-Job -Name $watchDirectoryJobName -ArgumentList $WatchDirectoryParameters -ScriptBlock {
        param([hashtable]$params)

        & "C:\tools\scripts\Watch-Directory.ps1" @params

    } | Out-Null

    Write-Host "$(Get-Date -Format $timeFormat): xConnect ENTRYPOINT: '$watchDirectoryJobName' started."
}
else
{
    Write-Host "$(Get-Date -Format $timeFormat): xConnect ENTRYPOINT: Skipping start of '$watchDirectoryJobName'. To enable you should mount a directory into 'C:\deploy'."
}

# run Sitecore's modified entrypoint script
Write-Host "$(Get-Date -Format $timeFormat): xConnect ENTRYPOINT: Run Sitecore's modified config script"
& "C:\tools\scripts\Configure-W3SVCService.ps1"

Write-Host "$(Get-Date -Format $timeFormat): xConnect ENTRYPOINT:Starting Service Monitor..."
Start-Service w3svc

# wait for w3wp to stop
while ($true)
{
    $processName = "w3wp"

    Write-Host "$(Get-Date -Format $timeFormat): Waiting for process '$processName' to stop..."

    $running = [array](Get-Process -Name $processName -ErrorAction "SilentlyContinue").Length -gt 0

    if ($running)
    {
        Stop-Process -Name $processName -Force -ErrorAction "SilentlyContinue"
    }
    else
    {
        Write-Host "$(Get-Date -Format $timeFormat): Process '$processName' stopped..."

        break;
    }

    Start-Sleep -Milliseconds 500
}

# wait for application pool to stop
Wait-WebItemState -IISPath "IIS:\AppPools\DefaultAppPool" -State "Stopped"

# start ServiceMonitor.exe in background, kill foreground process if it fails
Start-Job -Name "ServiceMonitor.exe" {
    try
    {
        & "C:\ServiceMonitor.exe" "w3svc"
    }
    finally
    {
        Get-Process -Name "filebeat" | Stop-Process -Force
    }
} | Out-Null

# wait for the ServiceMonitor.exe process is running
while ($true)
{
    $processName = "ServiceMonitor"

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
$serilogFilePath = "C:/inetpub/wwwroot/app_data/config/sitecore/coreservices/sc.serilog.xml"
if($useWatchDirectory){
    $serilogFilePath = "C:/deploy/app_data/config/sitecore/coreservices/sc.serilog.xml"
}
& "C:\tools\scripts\Set-Config.ps1" `
    -AppInsightsRoleName $env:SITECORE_APPSETTINGS_AppInsightsRoleName `
    -SerilogFilePath $serilogFilePath

# wait for application pool to start
Wait-WebItemState -IISPath "IIS:\AppPools\DefaultAppPool" -State "Started"

# print ready message
Write-Host ("$(Get-Date -Format $timeFormat): xConnect ready!")

# start filebeat.exe in foreground
& "C:\tools\bin\filebeat\filebeat.exe" -c (Join-Path $PSScriptRoot "\filebeat.yml")