using LCH.Abp.Elsa.Notifications;
using LCH.Abp.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.Elsa.Tests
{
    [DependsOn(
        typeof(AbpElsaNotificationsModule),
        typeof(AbpTestsBaseModule)
        )]
    public class AbpElsaTestsModule : AbpModule
    {
    }
}
