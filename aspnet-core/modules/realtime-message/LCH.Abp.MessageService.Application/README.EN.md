# LCH.Abp.MessageService.Application

Message service application layer module.

## Features

* Implements application layer interfaces for message service
* Implements business logic for message service
* Supports automatic object mapping

## Dependencies

* [LCH.Abp.MessageService.Application.Contracts](../LCH.Abp.MessageService.Application.Contracts/README.EN.md)
* [LCH.Abp.MessageService.Domain](../LCH.Abp.MessageService.Domain/README.EN.md)

## Installation

1. First, install the LCH.Abp.MessageService.Application package to your project:

```bash
dotnet add package LCH.Abp.MessageService.Application
```

2. Add `AbpMessageServiceApplicationModule` to your module's dependency list:

```csharp
[DependsOn(typeof(AbpMessageServiceApplicationModule))]
public class YourModule : AbpModule
{
}
```

## More

[中文文档](README.md)
