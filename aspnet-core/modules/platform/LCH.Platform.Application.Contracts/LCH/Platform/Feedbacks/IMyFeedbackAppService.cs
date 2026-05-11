using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LCH.Platform.Feedbacks;
public interface IMyFeedbackAppService : IApplicationService
{
    Task<PagedResultDto<FeedbackDto>> GetMyFeedbacksAsync(FeedbackGetListInput input);
}
