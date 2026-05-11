using Microsoft.Extensions.Hosting;
using SkyApm;

namespace LCH.Abp.Telemetry.SkyWalking.Hosting;

internal class HostingEnvironmentProvider : IEnvironmentProvider
{
    public string EnvironmentName { get; }

    public HostingEnvironmentProvider(IHostEnvironment hostingEnvironment)
    {
        EnvironmentName = hostingEnvironment.EnvironmentName;
    }
}
