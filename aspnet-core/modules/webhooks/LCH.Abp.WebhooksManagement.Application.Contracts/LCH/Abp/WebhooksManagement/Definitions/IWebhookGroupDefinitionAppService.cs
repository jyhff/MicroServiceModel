using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LCH.Abp.WebhooksManagement.Definitions;

public interface IWebhookGroupDefinitionAppService : IApplicationService
{
    Task<WebhookGroupDefinitionDto> GetAsync(string name);

    Task DeleteAsync(string name);

    Task<WebhookGroupDefinitionDto> CreateAsync(WebhookGroupDefinitionCreateDto input);

    Task<WebhookGroupDefinitionDto> UpdateAsync(string name, WebhookGroupDefinitionUpdateDto input);

    Task<ListResultDto<WebhookGroupDefinitionDto>> GetListAsync(WebhookGroupDefinitionGetListInput input);
}
