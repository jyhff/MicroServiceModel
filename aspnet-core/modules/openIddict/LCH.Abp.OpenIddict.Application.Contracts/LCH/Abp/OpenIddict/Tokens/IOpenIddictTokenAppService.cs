using System;
using Volo.Abp.Application.Services;

namespace LCH.Abp.OpenIddict.Tokens;

public interface IOpenIddictTokenAppService :
    IReadOnlyAppService<
        OpenIddictTokenDto,
        Guid,
        OpenIddictTokenGetListInput>,
    IDeleteAppService<Guid>
{
}
