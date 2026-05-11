using LCH.Abp.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.Notifications
{
    [DependsOn(
        typeof(AbpNotificationsModule),
        typeof(AbpTestsBaseModule))]
    public class AbpNotificationsTestsModule : AbpModule
    {
    }
}
