# escape=`

ARG BASE_IMAGE
ARG TOOLING_IMAGE

FROM ${TOOLING_IMAGE} as tools_resources
FROM ${BASE_IMAGE} as base

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

##########################
# New Relic configurations
##########################
# Set to CORECLR_ENABLE_PROFILING=1 to enable profiling
ENV CORECLR_ENABLE_PROFILING=0
ENV NEW_RELIC_LICENSE_KEY= 
ENV NEW_RELIC_APP_NAME=

RUN Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1')); `    
    choco install vim --version 8.2.2519 -y; `
    choco install far --version 3.0.5800 -y;`
    # download remote assets
    #New-Item -Path 'C:\\downloads' -ItemType 'Directory' -Force | Out-Null; `
    #& curl.exe -sS -L -o c:\\downloads\\nuget.exe https://dist.nuget.org/win-x86-commandline/v5.2.0/nuget.exe; `    
    #& curl.exe -sS -L -o C:\\downloads\\filebeat.zip https://artifacts.elastic.co/downloads/beats/filebeat/filebeat-7.4.1-windows-x86_64.zip; `
    # install nuget
    #New-Item -Path 'C:\\install\\tools\\bin' -ItemType 'Directory' -Force | Out-Null; `
    #Move-Item -Path 'C:\\downloads\\nuget.exe' -Destination 'C:\\install\\tools\\bin' -Force; `
    # # install serilog assembly
    # & 'C:\\install\\tools\\bin\\nuget.exe' install 'Serilog.Sinks.UDP' -Version '6.0.0' -OutputDirectory 'C:\\install\\serilog'; `
    # New-Item -Path 'c:\\install\\bin\\serilog' -ItemType 'Directory' -Force | Out-Null; `
    # Copy-Item -Path 'C:\\install\\serilog\\Serilog.Sinks.PeriodicBatching*\\lib\\netstandard2.0\\*.dll' -Destination 'C:\\install\\bin\\serilog\\'; `
    # Copy-Item -Path 'C:\\install\\serilog\\Serilog.Sinks.Udp*\\lib\\netstandard2.0\\*.dll' -Destination 'C:\\install\\bin\\serilog\\'; `    
    # Copy-Item -Path 'C:\\install\\bin\\serilog\\*.dll' -Destination 'C:\\inetpub\\wwwroot\\bin\\'; `
    # # install filebeat
    # Expand-Archive -Path 'C:\\downloads\\filebeat.zip' -DestinationPath 'C:\\tools\\bin' -Force; `
    # Rename-Item -Path (Get-Item -Path 'C:\\tools\\bin\\filebeat*windows*').FullName -NewName "filebeat"; `
    # Install New Relic 
    New-Item -Path 'C:\\downloads' -ItemType 'Directory' -Force | Out-Null; `
    & curl.exe -sS -L -o c:\\downloads\\newrelic-agent.msi https://download.newrelic.com/dot_net_agent/latest_release/newrelic-agent-win-x64-8.39.1.0.msi; `    
    Start-Process -Wait -FilePath msiexec -ArgumentList /i,c:\\downloads\\newrelic-agent.msi,/qn;  

COPY --from=tools_resources c:\tools c:\tools

# For custom xconnect artifacts
#COPY docker/build/assets/tools c:\tools\
#COPY artifacts/xconnect c:\inetpub\wwwroot

#ENTRYPOINT [ "powershell.exe", "c:/tools/entrypoints/Entrypoint.ps1" ]