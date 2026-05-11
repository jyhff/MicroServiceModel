using System.Threading.Tasks;

namespace LCH.Abp.Quartz.SqlInstaller;

public interface IQuartzSqlInstaller
{
    bool CanInstall(string driverDelegateType);

    Task InstallAsync();
}
