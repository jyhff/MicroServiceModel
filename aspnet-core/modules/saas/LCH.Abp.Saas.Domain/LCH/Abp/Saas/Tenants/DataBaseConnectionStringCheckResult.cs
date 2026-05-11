using System;
using Volo.Abp.Data;

namespace LCH.Abp.Saas.Tenants;

public class DataBaseConnectionStringCheckResult : AbpConnectionStringCheckResult
{
    public Exception Error { get; set; }
}
