using LCH.Abp.WeChat;
using LCH.Abp.WeChat.MiniProgram;
using LCH.Abp.WeChat.Official;

namespace LCH.Abp.OpenIddict.WeChat;

public static class WeChatTokenExtensionGrantConsts
{
    public static string MiniProgramGrantType => AbpWeChatMiniProgramConsts.GrantType;
    public static string OfficialGrantType => AbpWeChatOfficialConsts.GrantType;
    public static string ProfileKey => AbpWeChatGlobalConsts.ProfileKey;
}
