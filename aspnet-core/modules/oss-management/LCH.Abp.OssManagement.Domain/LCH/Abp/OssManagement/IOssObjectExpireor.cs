using System.Threading.Tasks;

namespace LCH.Abp.OssManagement;
public interface IOssObjectExpireor
{
    Task ExpireAsync(ExprieOssObjectRequest request);
}
