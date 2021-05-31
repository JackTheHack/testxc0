# escape=`

ARG BASE_IMAGE
ARG SOLUTION_IMAGE
ARG ROLE

FROM ${SOLUTION_IMAGE} as solution
FROM ${BASE_IMAGE}


SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# TODO: Copy CommerceEngine artifacts here from the solution image

#============ Tools ==================

USER ContainerAdministrator

ENV CORECLR_ENABLE_PROFILING=0
ENV NEW_RELIC_LICENSE_KEY= 
ENV NEW_RELIC_APP_NAME=

RUN Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1')); `
    #choco install urlrewrite --version 2.1.20190828 -y; `
    choco install vim --version 8.2.2519 -y; `
    choco install far --version 3.0.5800 -y;`
    # Install New Relic 
    New-Item -Path 'C:\\downloads' -ItemType 'Directory' -Force | Out-Null; `
    & curl.exe -sS -L -o c:\\downloads\\newrelic-agent.msi https://download.newrelic.com/dot_net_agent/latest_release/newrelic-agent-win-x64-8.39.1.0.msi; `    
    Start-Process -Wait -FilePath msiexec -ArgumentList /i,c:\\downloads\\newrelic-agent.msi,/qn; 

USER ContainerUser