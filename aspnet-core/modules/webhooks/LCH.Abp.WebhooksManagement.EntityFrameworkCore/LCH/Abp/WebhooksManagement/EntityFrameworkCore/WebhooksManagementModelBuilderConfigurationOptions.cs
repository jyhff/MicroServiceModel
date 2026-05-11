using JetBrains.Annotations;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace LCH.Abp.WebhooksManagement.EntityFrameworkCore;

public class WebhooksManagementModelBuilderConfigurationOptions : AbpModelBuilderConfigurationOptions
{
    public WebhooksManagementModelBuilderConfigurationOptions(
        [NotNull] string tablePrefix = "",
        [CanBeNull] string schema = null)
        : base(
            tablePrefix,
            schema)
    {

    }
}
