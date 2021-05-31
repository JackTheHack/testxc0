Param(    
    [Parameter(Mandatory=$false)]
    [string]$AppInsightsRoleName,
    [Parameter(Mandatory=$false)]
    [string]$SerilogFilePath
)

# Setup
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$timeFormat = "HH:mm:ss:fff"

$configFilePath = $SerilogFilePath
$config = [xml](Get-Content $configFilePath)

Write-Host "$(Get-Date -Format $timeFormat): Updating $configFilePath..."
if(![string]::IsNullOrEmpty($AppInsightsRoleName)){
  if($config.SelectSingleNode("/Settings/Serilog/Properties/Role") -eq $null){
    $roleElement = $config.CreateElement("Role") 
    $config.settings.serilog.properties.AppendChild($roleElement) 
  }  
  $config.settings.serilog.properties.Role = $AppInsightsRoleName  

  if($config.SelectSingleNode("/Settings/Serilog/Properties/Tag") -eq $null){
    $tagElement = $config.CreateElement("Tag") 
    $config.settings.serilog.properties.AppendChild($tagElement) 
  }  
  $config.settings.serilog.properties.Tag = "xconnect"
}
$config.Save($configFilePath)
Write-Host "$(Get-Date -Format $timeFormat): $configFilePath updated"