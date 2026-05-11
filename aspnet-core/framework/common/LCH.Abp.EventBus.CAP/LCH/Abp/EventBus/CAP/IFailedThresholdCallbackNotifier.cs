using System.Threading.Tasks;

namespace LCH.Abp.EventBus.CAP;

public interface IFailedThresholdCallbackNotifier
{
    Task NotifyAsync(AbpCAPExecutionFailedException exception);
}
