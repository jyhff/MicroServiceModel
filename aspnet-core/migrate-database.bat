@echo off
cls

call .\migrate-db-cmd.bat LCH.MicroService.Platform.DbMigrator platform --run
call .\migrate-db-cmd.bat LCH.MicroService.AuthServer.DbMigrator auth-server --run
call .\migrate-db-cmd.bat LCH.MicroService.IdentityServer.DbMigrator identityserver4-admin --run
call .\migrate-db-cmd.bat LCH.MicroService.LocalizationManagement.DbMigrator localization --run
call .\migrate-db-cmd.bat LCH.MicroService.RealtimeMessage.DbMigrator messages --run
call .\migrate-db-cmd.bat LCH.MicroService.TaskManagement.DbMigrator task-management --run
call .\migrate-db-cmd.bat LCH.MicroService.WebhooksManagement.DbMigrator webhooks-management --run
call .\migrate-db-cmd.bat LCH.MicroService.BackendAdmin.DbMigrator admin --run

taskkill /IM dotnet.exe /F