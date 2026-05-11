using JetBrains.Annotations;
using Volo.Abp.GlobalFeatures;

namespace LCH.Abp.AspNetCore.Mvc.GlobalFeatures
{
    public class AbpAspNetCoreMvcTestFeatures : GlobalModuleFeatures
    {
        public const string ModuleName = "AbpAspNetCoreMvcTest";

        public AbpAspNetCoreMvcTestFeatures([NotNull] GlobalFeatureManager featureManager)
            : base(featureManager)
        {
        }
    }
}
