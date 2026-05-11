using LCH.Abp.Gdpr.Localization;
using Volo.Abp.Application.Services;

namespace LCH.Abp.Gdpr;
public abstract class GdprApplicationServiceBase : ApplicationService
{
    protected GdprApplicationServiceBase()
    {
        LocalizationResource = typeof(GdprResource);
        ObjectMapperContext = typeof(AbpGdprApplicationModule);
    }
}
