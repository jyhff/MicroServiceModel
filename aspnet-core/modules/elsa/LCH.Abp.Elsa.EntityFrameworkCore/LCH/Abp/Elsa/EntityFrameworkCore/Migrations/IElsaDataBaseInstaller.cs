using System.Threading.Tasks;

namespace LCH.Abp.Elsa.EntityFrameworkCore.Migrations;

public interface IElsaDataBaseInstaller
{
    Task InstallAsync();
}
