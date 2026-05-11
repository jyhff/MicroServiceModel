# LCH.Abp.CachingManagement.Application.Contracts

Cache management application service contract module.

## Interfaces

### ICacheAppService

Cache management application service interface, providing the following features:

* `GetKeysAsync`: Get cache key list
* `GetValueAsync`: Get cache value
* `SetAsync`: Set cache value
* `RefreshAsync`: Refresh cache
* `RemoveAsync`: Remove cache

## Permissions

* AbpCachingManagement.Cache: Cache management
  * AbpCachingManagement.Cache.Refresh: Refresh cache
  * AbpCachingManagement.Cache.Delete: Delete cache
  * AbpCachingManagement.Cache.ManageValue: Manage cache value

## Installation

```bash
abp add-module LCH.Abp.CachingManagement
```

## More

For more information, please refer to the following resources:

* [Application Service Implementation](../LCH.Abp.CachingManagement.Application/README.EN.md)
* [HTTP API](../LCH.Abp.CachingManagement.HttpApi/README.EN.md)
* [Domain Layer](../LCH.Abp.CachingManagement.Domain/README.EN.md)
