using LCH.Abp.OssManagement.Localization;
using Volo.Abp.Application.Services;

namespace LCH.Abp.OssManagement;

public abstract class OssManagementApplicationServiceBase : ApplicationService
{
    protected OssManagementApplicationServiceBase()
    {
        LocalizationResource = typeof(AbpOssManagementResource);
        ObjectMapperContext = typeof(AbpOssManagementApplicationModule);
    }
}
