using Volo.Abp.Modularity;

namespace LCH.Abp.Encryption.SM4;

[DependsOn(typeof(AbpEncryptionSM4Module))]
public class AbpEncryptionSM4TestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }
}
