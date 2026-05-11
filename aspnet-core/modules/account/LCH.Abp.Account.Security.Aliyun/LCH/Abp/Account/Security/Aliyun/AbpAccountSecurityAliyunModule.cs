using LCH.Abp.Sms.Aliyun;
using Volo.Abp.Modularity;

namespace LCH.Abp.Account.Security.Aliyun;

[DependsOn(
    typeof(AbpAccountSecurityModule),
    typeof(AbpAliyunSmsModule))]
public class AbpAccountSecurityAliyunModule : AbpModule
{
}
