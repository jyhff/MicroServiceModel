using LCH.Abp.Tests;
using Volo.Abp.Json;
using Volo.Abp.Modularity;

namespace LCH.Abp.Notifications.Templating;

[DependsOn(
    typeof(AbpNotificationsTemplatingModule),
    typeof(AbpJsonModule),
    typeof(AbpTestsBaseModule))]
public class AbpNotificationsTemplatingTestModule : AbpModule
{

}
