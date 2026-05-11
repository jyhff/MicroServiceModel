using LCH.Abp.Identity.QrCode;

namespace LCH.Abp.Account.Web.Areas.Account.Controllers.Models;

public class QrCodeInfoResult
{
    public string Key { get; set; }
    public QrCodeStatus Status { get; set; }
}
