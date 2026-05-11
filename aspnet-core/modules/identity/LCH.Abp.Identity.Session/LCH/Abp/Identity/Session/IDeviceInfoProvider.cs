using System.Threading.Tasks;

namespace LCH.Abp.Identity.Session;
public interface IDeviceInfoProvider
{
    Task<DeviceInfo> GetDeviceInfoAsync();

    string ClientIpAddress { get; }
}
