using LCH.Abp.WeChat.Work.Authorize.Models;

namespace LCH.Abp.WeChat.Work.Authorize.Response;
internal static class WeChatWorkResponseExtensions
{
    public static WeChatWorkUserInfo ToUserInfo(
        this WeChatWorkUserInfoResponse response)
    {
        response.ThrowIfNotSuccess();

        return new WeChatWorkUserInfo
        {
            UserId = response.UserId,
            UserTicket = response.UserTicket,
        };
    }

    public static WeChatWorkUserDetail ToUserDetail(
        this WeChatWorkUserDetailResponse response)
    {
        response.ThrowIfNotSuccess();

        return new WeChatWorkUserDetail
        {
            UserId = response.UserId,
            Address = response.Address,
            Avatar = response.Avatar,
            QrCode = response.QrCode,
            Email = response.Email,
            Gender = response.Gender,
            Mobile = response.Mobile,
            WorkEmail = response.WorkEmail,
        };
    }
}
