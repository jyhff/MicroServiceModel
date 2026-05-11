using LCH.Abp.IM.Localization;
using LCH.Abp.MessageService.Chat;
using LCH.Abp.MessageService.Localization;
using LCH.Abp.MessageService.ObjectExtending;
using LCH.Abp.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Caching;
using Volo.Abp.Localization;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending.Modularity;

namespace LCH.Abp.MessageService;

[DependsOn(
    typeof(AbpMapperlyModule),
    typeof(AbpCachingModule),
    typeof(AbpNotificationsModule),
    typeof(AbpMessageServiceDomainSharedModule))]
public class AbpMessageServiceDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<AbpMessageServiceDomainModule>();

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<MessageServiceResource>()
                .AddBaseTypes(typeof(AbpIMResource));
        });
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
            MessageServiceModuleExtensionConsts.ModuleName,
            MessageServiceModuleExtensionConsts.EntityNames.Message,
            typeof(Message)
        );
    }
}
