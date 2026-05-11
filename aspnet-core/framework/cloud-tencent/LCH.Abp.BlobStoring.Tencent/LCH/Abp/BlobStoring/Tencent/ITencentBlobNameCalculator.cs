using Volo.Abp.BlobStoring;

namespace LCH.Abp.BlobStoring.Tencent;

public interface ITencentBlobNameCalculator
{
    string Calculate(BlobProviderArgs args);
}
