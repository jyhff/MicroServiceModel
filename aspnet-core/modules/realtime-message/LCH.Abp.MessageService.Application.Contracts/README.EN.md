# LCH.Abp.MessageService.Application.Contracts

Message service application layer contracts module.

## Features

* Defines application layer interfaces for message service
* Defines DTOs for message service
* Provides multi-language resources
* Supports virtual file system

## Dependencies

* [LCH.Abp.MessageService.Domain.Shared](../LCH.Abp.MessageService.Domain.Shared/README.EN.md)

## Installation

1. First, install the LCH.Abp.MessageService.Application.Contracts package to your project:

```bash
dotnet add package LCH.Abp.MessageService.Application.Contracts
```

2. Add `AbpMessageServiceApplicationContractsModule` to your module's dependency list:

```csharp
[DependsOn(typeof(AbpMessageServiceApplicationContractsModule))]
public class YourModule : AbpModule
{
}
```

## More

[中文文档](README.md)
