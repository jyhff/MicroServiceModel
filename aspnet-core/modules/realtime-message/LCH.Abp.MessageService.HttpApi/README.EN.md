# LCH.Abp.MessageService.HttpApi

Message service HTTP API module.

## Features

* Provides HTTP API interfaces for message service
* Supports MVC data annotation localization
* Automatic API controller registration

## Dependencies

* [LCH.Abp.MessageService.Application.Contracts](../LCH.Abp.MessageService.Application.Contracts/README.EN.md)
* `AbpAspNetCoreMvcModule`

## Installation

1. First, install the LCH.Abp.MessageService.HttpApi package to your project:

```bash
dotnet add package LCH.Abp.MessageService.HttpApi
```

2. Add `AbpMessageServiceHttpApiModule` to your module's dependency list:

```csharp
[DependsOn(typeof(AbpMessageServiceHttpApiModule))]
public class YourModule : AbpModule
{
}
```

## More

[中文文档](README.md)
