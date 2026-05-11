using Volo.Abp.Application.Services;

namespace LCH.Abp.TaskManagement;

public interface IBackgroundJobLogAppService : 
    IReadOnlyAppService<
        BackgroundJobLogDto,
        long,
        BackgroundJobLogGetListInput>,
    IDeleteAppService<long>
{
}
