# escape=`
# This Dockerfile is used to generate images for the following roles: mssql

ARG BASE_IMAGE

FROM ${BASE_IMAGE}

ENV DATA_PATH='C:\\data\\'

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY ./dacpacs C:\fs_db

RUN C:\DeployDatabases.ps1 -ResourcesDirectory C:\fs_db
RUN Remove-Item C:\fs_db -Recurse -Force;