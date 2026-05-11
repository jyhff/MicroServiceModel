using System.Threading.Tasks;

namespace LCH.Abp.Cli.Commands
{
    public interface ICreateProjectService
    {
        Task CreateAsync(ProjectCreateArgs createArgs);
    }
}
