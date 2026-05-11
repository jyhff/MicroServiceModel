using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.PermissionManagement.Localization;

namespace LCH.Abp.PermissionManagement.HttpApi;
public abstract class PermissionManagementControllerBase : AbpControllerBase
{
    protected PermissionManagementControllerBase()
    {
        LocalizationResource = typeof(AbpPermissionManagementResource);
    }
}
