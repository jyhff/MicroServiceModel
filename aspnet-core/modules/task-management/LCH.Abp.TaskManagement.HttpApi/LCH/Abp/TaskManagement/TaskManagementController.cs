using LCH.Abp.TaskManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace LCH.Abp.TaskManagement;

public abstract class TaskManagementController : AbpControllerBase
{
    protected TaskManagementController()
    {
        LocalizationResource = typeof(TaskManagementResource);
    }
}
