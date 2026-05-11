@echo off
cls
chcp 65001

echo. 清理所有服务日志

del .\services\LCH.MicroService.Applications.Single\Logs /Q
del .\services\LCH.MicroService.BackendAdmin.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.AuthServer\Logs /Q
del .\services\LCH.MicroService.AuthServer.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.identityServer\Logs /Q
del .\services\LCH.MicroService.identityServer.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.LocalizationManagement.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.PlatformManagement.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.RealtimeMessage.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.TaskManagement.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.WebhooksManagement.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.WechatManagement.HttpApi.Host\Logs /Q
del .\services\LCH.MicroService.WorkflowManagement.HttpApi.Host\Logs /Q

