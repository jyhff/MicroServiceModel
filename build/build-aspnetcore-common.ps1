# COMMON PATHS 

$rootFolder = (Get-Item -Path "./" -Verbose).FullName

# List of solutions used only in development mode
[PsObject[]]$serviceArray = @()

$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.BackendAdmin.HttpApi.Host/"; Service = "admin-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.AuthServer/"; Service = "auth-server" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.AuthServer.HttpApi.Host/"; Service = "auth-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.identityServer/"; Service = "identity-server" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.identityServer.HttpApi.Host/"; Service = "identity-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.LocalizationManagement.HttpApi.Host/"; Service = "localization-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.PlatformManagement.HttpApi.Host/"; Service = "platform-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.RealtimeMessage.HttpApi.Host/"; Service = "message-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.TaskManagement.HttpApi.Host/"; Service = "task-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.WebhooksManagement.HttpApi.Host/"; Service = "webhook-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.WorkflowManagement.HttpApi.Host/"; Service = "workflow-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/services/LCH.MicroService.WechatManagement.HttpApi.Host/"; Service = "wechat-service" }
$serviceArray += [PsObject]@{ Path = $rootFolder + "/../gateways/internal/LCH.MicroService.Internal.ApiGateway/src/LCH.MicroService.Internal.Gateway/"; Service = "internal-apigateway" }

[PsObject[]]$solutionArray = @()
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LCH.MicroService.All.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LCH.MicroService.Common.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LCH.MicroService.TaskManagement.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LCH.MicroService.WebhooksManagement.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LCH.MicroService.Workflow.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LCH.MicroService.SingleProject.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../aspnet-core/LCH.MicroService.WechatManagement.sln" }
$solutionArray += [PsObject]@{ File = $rootFolder + "/../gateways/internal/LCH.MicroService.Internal.ApiGateway/LCH.MicroService.Internal.ApiGateway.sln" }

[PsObject[]]$migrationArray = @()
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.Platform.DbMigrator" }
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.LocalizationManagement.DbMigrator" }
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.RealtimeMessage.DbMigrator" }
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.IdentityServer.DbMigrator" }
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.TaskManagement.DbMigrator" }
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.AuthServer.DbMigrator" }
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.WebhooksManagement.DbMigrator" }
$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.BackendAdmin.DbMigrator" }
#$migrationArray += [PsObject]@{ Path = $rootFolder + "/../aspnet-core/migrations/LCH.MicroService.Applications.Single.DbMigrator" }

Write-host ""
Write-host ":::::::::::::: !!! You are in development mode !!! ::::::::::::::" -ForegroundColor red -BackgroundColor  yellow
Write-host "" 
