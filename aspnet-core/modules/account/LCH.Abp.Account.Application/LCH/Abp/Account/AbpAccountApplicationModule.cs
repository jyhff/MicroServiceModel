using LCH.Abp.Account.Security;
using LCH.Abp.Account.Security.Localization;
using LCH.Abp.Identity;
using LCH.Abp.WeChat.MiniProgram;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account.Localization;
using Volo.Abp.BlobStoring;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace LCH.Abp.Account;

[DependsOn(
    typeof(Volo.Abp.Account.AbpAccountApplicationModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpAccountSecurityModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpBlobStoringModule),
    typeof(AbpWeChatMiniProgramModule))]
public class AbpAccountApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<AbpAccountApplicationModule>();

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AbpAccountApplicationModule>();
        });

        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].Urls[AccountUrlNames.EmailConfirm] = "Account/EmailConfirm";
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<AccountResource>()
                .AddBaseTypes(typeof(AccountSecurityResource));
        });
    }
}
