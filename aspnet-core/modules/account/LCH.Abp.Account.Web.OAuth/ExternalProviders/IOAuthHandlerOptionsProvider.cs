using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace LCH.Abp.Account.Web.OAuth.ExternalProviders;

public interface IOAuthHandlerOptionsProvider<TOptions>
    where TOptions : RemoteAuthenticationOptions, new()
{
    Task SetOptionsAsync(TOptions options);
}
