using System.Threading.Tasks;

namespace LCH.Abp.DistributedLocking.Dapr;

public interface ILockOwnerFinder
{
    Task<string> FindAsync();
}
