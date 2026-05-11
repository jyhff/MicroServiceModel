using LCH.Platform.Messages;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.Settings;

namespace LCH.MicroService.PlatformManagement.Messages;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IEmailMessageManager), typeof(EmailMessageManager))]
public class PlatformEmailMessageManager : EmailMessageManager
{
    public PlatformEmailMessageManager(
        ISettingProvider settingProvider,
        IEmailMessageRepository repository, 
        IBlobContainer<MessagingContainer> blobContainer) 
        : base(settingProvider, repository, blobContainer)
    {
    }

    protected override IEmailSender GetEmailSender()
    {
        return LazyServiceProvider.GetRequiredKeyedService<IEmailSender>("DefaultEmailSender");
    }
}
