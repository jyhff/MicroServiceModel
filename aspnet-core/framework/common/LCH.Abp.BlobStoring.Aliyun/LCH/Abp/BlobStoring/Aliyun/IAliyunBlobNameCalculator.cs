using Volo.Abp.BlobStoring;

namespace LCH.Abp.BlobStoring.Aliyun;

public interface IAliyunBlobNameCalculator
{
    string Calculate(BlobProviderArgs args);
}
