using System.ComponentModel.DataAnnotations;

namespace LCH.Abp.Account.Web.OAuth.Areas.Account.Controllers.Dtos;

public class WorkWeixinLoginBindInput
{
    [Required]
    [Display(Name = "WorkWeixin:Code")]
    public string Code { get; set; }
}
