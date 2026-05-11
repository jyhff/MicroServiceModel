using LCH.Abp.Aliyun;

namespace LCH.Abp.Sms.Aliyun;

public class AliyunSmsException : AbpAliyunException
{
    public AliyunSmsException(string code, string message)
        :base(code, message)
    {
    }
}
