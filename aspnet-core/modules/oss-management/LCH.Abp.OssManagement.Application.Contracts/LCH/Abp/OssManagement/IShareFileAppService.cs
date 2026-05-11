using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace LCH.Abp.OssManagement;

public interface IShareFileAppService : IApplicationService
{
    Task<IRemoteStreamContent> GetAsync(string url);
}
