using COSXML;
using System.Threading.Tasks;

namespace LCH.Abp.BlobStoring.Tencent;

public interface ICosClientFactory
{
    Task<CosXml> CreateAsync<TContainer>();

    Task<CosXml> CreateAsync(TencentBlobProviderConfiguration configuration);

    Task<TencentBlobProviderConfiguration> GetConfigurationAsync<TContainer>();
}
