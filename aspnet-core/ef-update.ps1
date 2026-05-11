Set-Location ".\migrations\LCH.MicroService.BackendAdmin.EntityFrameworkCore"
dotnet ef database update

Set-Location "..\LCH.MicroService.Platform.EntityFrameworkCore"
dotnet ef database update

Set-Location "..\LCH.MicroService.LocalizationManagement.EntityFrameworkCore"
dotnet ef database update

Set-Location "..\LCH.MicroService.RealtimeMessage.EntityFrameworkCore"
dotnet ef database update

Set-Location "..\LCH.MicroService.IdentityServer.EntityFrameworkCore"
dotnet ef database update

Set-Location "..\LCH.MicroService.TaskManagement.EntityFrameworkCore"
dotnet ef database update

Set-Location "..\LCH.MicroService.AuthServer.EntityFrameworkCore"
dotnet ef database update
