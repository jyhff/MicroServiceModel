using Volo.Abp.Modularity;
using Volo.Abp.Security;

namespace LCH.Abp.Encryption.SM4;

[DependsOn(typeof(AbpSecurityModule))]
public class AbpEncryptionSM4Module : AbpModule
{

}
