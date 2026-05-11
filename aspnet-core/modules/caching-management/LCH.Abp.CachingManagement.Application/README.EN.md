# LCH.Abp.CachingManagement.Application

Implementation of the cache management application service module.

## Features

* Implements the `ICacheAppService` interface, providing basic cache management functions:
  * Get cache key list
  * Get cache value
  * Set cache value
  * Refresh cache
  * Delete cache

## Permissions

* AbpCachingManagement.Cache: Cache management
  * AbpCachingManagement.Cache.Refresh: Refresh cache
  * AbpCachingManagement.Cache.Delete: Delete cache
  * AbpCachingManagement.Cache.ManageValue: Manage cache value

## Installation

```bash
abp add-module LCH.Abp.CachingManagement
```

## Configuration and Usage

1. First, you need to install the corresponding cache implementation module, for example: [LCH.Abp.CachingManagement.StackExchangeRedis](../LCH.Abp.CachingManagement.StackExchangeRedis/README.EN.md)

2. Add the following code in the `ConfigureServices` method of your module:

```csharp
Configure<AbpAutoMapperOptions>(options =>
{
    options.AddProfile<AbpCachingManagementApplicationAutoMapperProfile>(validate: true);
});
```

## More

For more information, please refer to the following resources:

* [Application Service Contracts](../LCH.Abp.CachingManagement.Application.Contracts/README.EN.md)
* [HTTP API](../LCH.Abp.CachingManagement.HttpApi/README.EN.md)
* [Domain Layer](../LCH.Abp.CachingManagement.Domain/README.EN.md)
