# escape=`

ARG BASE_IMAGE
ARG TOOLING_IMAGE
ARG SOLUTION_IMAGE

FROM ${SOLUTION_IMAGE} as solution
FROM ${TOOLING_IMAGE} as tooling
FROM ${BASE_IMAGE}


SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Copy development tools and entrypoint
COPY --from=tooling \tools\ \tools\

WORKDIR C:\inetpub\wwwroot

# Copy solution website files
COPY --from=solution \artifacts\website\ .\

# Copy solution transforms
COPY --from=solution \artifacts\transforms\ \transforms\solution\

# Copy role transforms
COPY .\transforms\ \transforms\role\

# Perform solution transforms
RUN C:\tools\scripts\Invoke-XdtTransform.ps1 -Path .\ -XdtPath C:\transforms\solution\DockerExamples.Website

# Perform role transforms
RUN C:\tools\scripts\Invoke-XdtTransform.ps1 -Path .\ -XdtPath C:\transforms\role

#============ Tools ==================

ENV CORECLR_ENABLE_PROFILING=0
ENV NEW_RELIC_LICENSE_KEY= 
ENV NEW_RELIC_APP_NAME=

RUN Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1')); `
    choco install urlrewrite --version 2.1.20190828 -y; `
    choco install vim --version 8.2.2519 -y; `
    # Install New Relic 
    New-Item -Path 'C:\\downloads' -ItemType 'Directory' -Force | Out-Null; `
    & curl.exe -sS -L -o c:\\downloads\\newrelic-agent.msi https://download.newrelic.com/dot_net_agent/latest_release/newrelic-agent-win-x64-8.39.1.0.msi; `    
    Start-Process -Wait -FilePath msiexec -ArgumentList /i,c:\\downloads\\newrelic-agent.msi,/qn; 
