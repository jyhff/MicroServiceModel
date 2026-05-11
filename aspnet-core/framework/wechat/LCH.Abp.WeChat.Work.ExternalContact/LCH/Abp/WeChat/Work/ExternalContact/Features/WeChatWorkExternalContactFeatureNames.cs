using LCH.Abp.WeChat.Work.Features;

namespace LCH.Abp.WeChat.Work.ExternalContact.Features;
/// <summary>
/// 企业微信客户联系模块功能
/// </summary>
public static class WeChatWorkExternalContactFeatureNames
{
    public const string GroupName = WeChatWorkFeatureNames.GroupName + ".ExternalContact";
    /// <summary>
    /// 启用企业微信客户联系
    /// </summary>
    public const string Enable = GroupName + ".Enable";
}
