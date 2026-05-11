@echo off
cls

call .\migrate-db-cmd.bat LCH.MicroService.Platform.EntityFrameworkCore platform --ef-u
call .\migrate-db-cmd.bat LCH.MicroService.BackendAdmin.EntityFrameworkCore admin --ef-u
call .\migrate-db-cmd.bat LCH.MicroService.AuthServer.EntityFrameworkCore auth-server --ef-u
call .\migrate-db-cmd.bat LCH.MicroService.IdentityServer.EntityFrameworkCore identityserver4-admin --ef-u
call .\migrate-db-cmd.bat LCH.MicroService.LocalizationManagement.EntityFrameworkCore localization --ef-u
call .\migrate-db-cmd.bat LCH.MicroService.RealtimeMessage.EntityFrameworkCore messages --ef-u
call .\migrate-db-cmd.bat LCH.MicroService.TaskManagement.EntityFrameworkCore task-management --ef-u
call .\migrate-db-cmd.bat LCH.MicroService.WebhooksManagement.EntityFrameworkCore webhooks-management --ef-u

taskkill /IM dotnet.exe /F