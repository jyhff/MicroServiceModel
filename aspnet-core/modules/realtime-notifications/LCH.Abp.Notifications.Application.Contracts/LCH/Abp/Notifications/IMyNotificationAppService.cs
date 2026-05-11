using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace LCH.Abp.Notifications;

public interface IMyNotificationAppService :
    IReadOnlyAppService<
        UserNotificationDto,
        long,
        UserNotificationGetByPagedDto
        >,
    IDeleteAppService<long>
{
    Task MarkReadStateAsync(NotificationMarkReadStateInput input);
}
