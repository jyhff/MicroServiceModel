# LCH.Abp.MessageService.HttpApi.Client

Message service HTTP API client module.

## Features

* Provides HTTP client proxies for message service
* Automatic HTTP client proxy service registration

## Dependencies

* [LCH.Abp.MessageService.Application.Contracts](../LCH.Abp.MessageService.Application.Contracts/README.EN.md)
* `AbpHttpClientModule`

## Installation

1. First, install the LCH.Abp.MessageService.HttpApi.Client package to your project:

```bash
dotnet add package LCH.Abp.MessageService.HttpApi.Client
```

2. Add `AbpMessageServiceHttpApiClientModule` to your module's dependency list:

```csharp
[DependsOn(typeof(AbpMessageServiceHttpApiClientModule))]
public class YourModule : AbpModule
{
}
```

3. Configure remote service URL:

```json
{
  "RemoteServices": {
    "AbpMessageService": {
      "BaseUrl": "http://your-service-url"
    }
  }
}
```

## More

[中文文档](README.md)
