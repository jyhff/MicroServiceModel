# LCH.Platform

Platform management module that provides a complete set of platform management features, including menu management, layout management, data dictionary, package management, and more.

## Features

* Menu Management
  * Multi-level menu structure
  * User menu customization
  * Role-based menu permissions
  * Menu favorites
  * Dynamic menu presets

* Layout Management
  * Layout view entities
  * Layout data association
  * Multi-framework support

* Data Dictionary
  * Data dictionary management
  * Dictionary item management
  * Dictionary seed data

* Package Management
  * Package version control
  * Package file management
  * Blob storage integration
  * Package filtering specifications

* VueVbenAdmin Integration
  * Theme settings
  * Layout settings
  * Menu settings
  * Header settings
  * Multi-tab settings

## Project Structure

* `LCH.Platform.Domain.Shared`: Shared domain layer
* `LCH.Platform.Domain`: Domain layer
* `LCH.Platform.EntityFrameworkCore`: Data access layer
* `LCH.Platform.Application.Contracts`: Application service contracts layer
* `LCH.Platform.Application`: Application service implementation layer
* `LCH.Platform.HttpApi`: HTTP API layer
* `LCH.Platform.HttpApi.Client`: HTTP API Proxy layer
* `LCH.Platform.Settings.VueVbenAdmin`: VueVbenAdmin frontend framework settings module

## Quick Start

1. Reference the modules
```csharp
[DependsOn(
    typeof(PlatformDomainModule),
    typeof(PlatformApplicationModule),
    typeof(PlatformHttpApiModule),
    typeof(PlatformSettingsVueVbenAdminModule)
)]
public class YouProjectModule : AbpModule
{
    // other
}
```

2. Configure the database
```json
{
  "ConnectionStrings": {
    "Platform": "Server=localhost;Database=Platform;Trusted_Connection=True"
  }
}
```

3. Update the database
```bash
dotnet ef database update
```

## Important Notes

1. Dynamic Menu Management
   * The module initializes vue-admin related menus by default
   * Menu data can be preset through the `IDataSeedContributor` interface
   * Layout (path) and menu (component) do not need the @/ prefix

2. Database Migration
   * Please execute database migration before running the platform service
   * Use the `dotnet ef database update` command to update the database structure

## More Information

* [Shared Domain Layer](./LCH.Platform.Domain.Shared/README.EN.md)
* [Domain Layer](./LCH.Platform.Domain/README.EN.md)
* [Data Access Layer](./LCH.Platform.EntityFrameworkCore/README.EN.md)
* [Application Service Contracts Layer](./LCH.Platform.Application.Contracts/README.EN.md)
* [Application Service Implementation Layer](./LCH.Platform.Application/README.EN.md)
* [HTTP API Layer](./LCH.Platform.HttpApi/README.EN.md)
* [VueVbenAdmin Settings Module](./LCH.Platform.Settings.VueVbenAdmin/README.EN.md)
