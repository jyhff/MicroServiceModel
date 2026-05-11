using LCH.Abp.WeChat.Work.Settings;

namespace LCH.Abp.WeChat.Work.ExternalContact.Settings;

public static class WeChatWorkExternalContactSettingNames
{
    public const string Prefix = WeChatWorkSettingNames.Prefix + ".ExternalContact";
    /// <summary>
    /// 客户联系应用Srcret
    /// </summary>
    public const string Secret = Prefix + ".Secret";
}
