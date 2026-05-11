@echo off
cls
set stime=8

start .\start-http-api-host.bat LCH.MicroService.IdentityServer identityserver --run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.IdentityServer.HttpApi.Host identityserver4-admin --run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.LocalizationManagement.HttpApi.Host localization --run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.PlatformManagement.HttpApi.Host platform --run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.RealtimeMessage.HttpApi.Host messages --run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.TaskManagement.HttpApi.Host task-management --run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.WebhooksManagement.HttpApi.Host webhooks-management--run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.WorkflowManagement.HttpApi.Host workflow-management --run
ping -n %stime% 127.1 >nul
start .\start-http-api-host.bat LCH.MicroService.BackendAdmin.HttpApi.Host admin --run
ping -n %stime% 127.1 >nul
start .\start-internal-gateway.bat --run