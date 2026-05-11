# LCH.Abp.Dynamic.Queryable.Application.Contracts

Dynamic query application service contract module, defining interfaces and DTOs related to dynamic querying.

## Features

* Defines dynamic query application service interface `IDynamicQueryableAppService<TEntityDto>`
* Provides DTO definitions for dynamic querying
* Supports parameter options and comparison operator definitions

## Configuration and Usage

1. Install the `LCH.Abp.Dynamic.Queryable.Application.Contracts` NuGet package

2. Add `[DependsOn(typeof(AbpDynamicQueryableApplicationContractsModule))]` to your module class

### Interface Description

```csharp
public interface IDynamicQueryableAppService<TEntityDto>
{
    // Get available fields list
    Task<ListResultDto<DynamicParamterDto>> GetAvailableFieldsAsync();

    // Query data based on dynamic conditions
    Task<PagedResultDto<TEntityDto>> SearchAsync(GetListByDynamicQueryableInput dynamicInput);
}
```

### DTO Description

* `DynamicParamterDto` - Dynamic parameter DTO
  * `Name` - Field name
  * `Type` - Field type
  * `Description` - Field description
  * `JavaScriptType` - JavaScript type
  * `AvailableComparator` - Available comparison operators
  * `Options` - Parameter options (for enum types)

* `ParamterOptionDto` - Parameter option DTO
  * `Key` - Option key
  * `Value` - Option value

* `GetListByDynamicQueryableInput` - Dynamic query input DTO
  * `SkipCount` - Number of records to skip
  * `MaxResultCount` - Maximum number of records to return
  * `Queryable` - Query conditions

## Related Links

* [LCH.Linq.Dynamic.Queryable](../LCH.Linq.Dynamic.Queryable/README.EN.md)
* [LCH.Abp.Dynamic.Queryable.Application](../LCH.Abp.Dynamic.Queryable.Application/README.EN.md)
