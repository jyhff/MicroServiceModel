using LCH.Abp.TaskManagement.Localization;
using Volo.Abp.Application.Services;

namespace LCH.Abp.TaskManagement;

public abstract class TaskManagementApplicationService : ApplicationService
{
    protected TaskManagementApplicationService()
    {
        LocalizationResource = typeof(TaskManagementResource);
        ObjectMapperContext = typeof(TaskManagementApplicationModule);
    }
}
