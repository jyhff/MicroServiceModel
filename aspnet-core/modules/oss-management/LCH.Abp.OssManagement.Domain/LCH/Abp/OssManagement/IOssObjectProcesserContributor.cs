using System.Threading.Tasks;

namespace LCH.Abp.OssManagement;

public interface IOssObjectProcesserContributor
{
    Task ProcessAsync(OssObjectProcesserContext context);
}
