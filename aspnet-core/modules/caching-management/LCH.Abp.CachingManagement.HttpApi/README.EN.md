# LCH.Abp.CachingManagement.HttpApi

HTTP API implementation for the cache management module.

## API Endpoints

### /api/caching-management/cache

* GET `/api/caching-management/cache/keys`: Get cache key list
  * Parameters:
    * prefix (string, optional): Key prefix
    * filter (string, optional): Filter condition
    * marker (string, optional): Pagination marker
  * Permission: AbpCachingManagement.Cache

* GET `/api/caching-management/cache/{key}`: Get cache value for specified key
  * Parameters:
    * key (string, required): Cache key
  * Permission: AbpCachingManagement.Cache

* POST `/api/caching-management/cache`: Set cache value
  * Parameters:
    * key (string, required): Cache key
    * value (string, required): Cache value
    * absoluteExpiration (DateTime, optional): Absolute expiration time
    * slidingExpiration (DateTime, optional): Sliding expiration time
  * Permission: AbpCachingManagement.Cache.ManageValue

* PUT `/api/caching-management/cache/refresh`: Refresh cache
  * Parameters:
    * key (string, required): Cache key
    * absoluteExpiration (DateTime, optional): Absolute expiration time
    * slidingExpiration (DateTime, optional): Sliding expiration time
  * Permission: AbpCachingManagement.Cache.Refresh

* DELETE `/api/caching-management/cache`: Delete cache
  * Parameters:
    * key (string, required): Cache key
  * Permission: AbpCachingManagement.Cache.Delete

## Installation

```bash
abp add-module LCH.Abp.CachingManagement
```

## More

For more information, please refer to the following resources:

* [Application Service Implementation](../LCH.Abp.CachingManagement.Application/README.EN.md)
* [Application Service Contracts](../LCH.Abp.CachingManagement.Application.Contracts/README.EN.md)
* [Domain Layer](../LCH.Abp.CachingManagement.Domain/README.EN.md)
