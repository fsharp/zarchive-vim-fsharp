@echo off
cls

paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

packages\FAKE\tools\FAKE.exe install.fsx %*

