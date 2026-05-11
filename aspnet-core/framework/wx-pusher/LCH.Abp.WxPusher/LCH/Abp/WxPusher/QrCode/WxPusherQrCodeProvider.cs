using JetBrains.Annotations;
using LCH.Abp.WxPusher.Features;
using LCH.Abp.WxPusher.Token;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Features;

namespace LCH.Abp.WxPusher.QrCode;

[RequiresFeature(WxPusherFeatureNames.Enable)]
public class WxPusherQrCodeProvider : WxPusherRequestProvider, IWxPusherQrCodeProvider
{
    protected IWxPusherTokenProvider WxPusherTokenProvider { get; }

    public WxPusherQrCodeProvider(IWxPusherTokenProvider wxPusherTokenProvider)
    {
        WxPusherTokenProvider = wxPusherTokenProvider;
    }

    public async virtual Task<CreateQrcodeResult> CreateQrcodeAsync(
        [NotNull] string extra, 
        int validTime = 1800, 
        CancellationToken cancellationToken = default)
    {
        var token = await WxPusherTokenProvider.GetTokenAsync(cancellationToken);
        var client = HttpClientFactory.GetWxPusherClient();
        var request = new CreateQrcodeRequest(token, extra, validTime);

        var content = await client.CreateQrcodeAsync(
            request,
            cancellationToken);

        var response = JsonSerializer.Deserialize<WxPusherResult<CreateQrcodeResult>>(content);

        return response.GetData();
    }

    public async virtual Task<GetScanQrCodeResult> GetScanQrCodeAsync(
        [NotNull] string code, 
        CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code));

        var client = HttpClientFactory.GetWxPusherClient();

        var content = await client.GetScanQrCodeUidAsync(
            code,
            cancellationToken);

        var response = JsonSerializer.Deserialize<WxPusherResult<GetScanQrCodeResult>>(content);

        return response.GetData();
    }
}
