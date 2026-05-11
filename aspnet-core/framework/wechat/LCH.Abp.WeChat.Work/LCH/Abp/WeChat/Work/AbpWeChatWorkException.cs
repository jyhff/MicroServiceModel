using System;
using Volo.Abp;

namespace LCH.Abp.WeChat.Work;

public class AbpWeChatWorkException : BusinessException
{
    public AbpWeChatWorkException()
    {
    }

    public AbpWeChatWorkException(
        string code = null,
        string message = null,
        string details = null,
        Exception innerException = null)
        : base(code, message, details, innerException)
    {
    }
}
