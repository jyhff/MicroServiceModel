using LCH.Abp.WeChat.Official.Services.Models;
using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.WeChat.Official.Services;
/// <summary>
/// 客服中心消息接口
/// </summary>
public interface IServiceCenterMessageSender
{
    Task SendAsync(MessageModel message, CancellationToken cancellationToken = default);
}
