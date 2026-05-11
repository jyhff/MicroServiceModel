# LCH.Abp.IM.SignalR

Instant messaging module implemented based on SignalR.

## Features

* Message sender provider implemented with SignalR
* Integration with ABP SignalR module
* Multi-language support

## Dependencies

* [LCH.Abp.IM](../LCH.Abp.IM/README.EN.md)
* `AbpAspNetCoreSignalRModule`

## Installation

1. First, install the LCH.Abp.IM.SignalR package to your project:

```bash
dotnet add package LCH.Abp.IM.SignalR
```

2. Add `AbpIMSignalRModule` to your module's dependency list:

```csharp
[DependsOn(typeof(AbpIMSignalRModule))]
public class YourModule : AbpModule
{
}
```

## More

[中文文档](README.md)
