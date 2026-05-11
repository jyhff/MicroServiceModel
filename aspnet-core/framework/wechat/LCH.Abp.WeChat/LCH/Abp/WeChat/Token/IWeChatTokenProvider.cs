using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.WeChat.Token;

public interface IWeChatTokenProvider
{
    Task<WeChatToken> GetTokenAsync(string appId, string appSecret, CancellationToken cancellationToken = default);
}
