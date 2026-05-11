using Volo.Abp.Modularity;

namespace LCH.Abp.Tencent.TTS;

[DependsOn(
    typeof(AbpTencentCloudModule))]
public class AbpTencentTTSModule : AbpModule
{
    //TencentCloud.Tts.V20190823.TtsClient
}
