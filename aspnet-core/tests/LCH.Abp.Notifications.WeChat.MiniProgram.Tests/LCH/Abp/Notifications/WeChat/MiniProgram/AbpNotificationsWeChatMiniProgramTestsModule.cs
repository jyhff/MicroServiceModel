using LCH.Abp.Tests;
using Volo.Abp.Modularity;

namespace LCH.Abp.Notifications.WeChat.MiniProgram
{
    [DependsOn(
        typeof(AbpNotificationsWeChatMiniProgramModule),
        typeof(AbpNotificationsTestsModule),
        typeof(AbpTestsBaseModule))]
    public class AbpNotificationsWeChatMiniProgramTestsModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure<AbpNotificationsWeChatMiniProgramOptions>(options =>
            {
                // options.DefaultMsgPrefix = "[wmp-override]";
            });
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpNotificationsPublishOptions>(options =>
            {
            });
        }
    }
}
