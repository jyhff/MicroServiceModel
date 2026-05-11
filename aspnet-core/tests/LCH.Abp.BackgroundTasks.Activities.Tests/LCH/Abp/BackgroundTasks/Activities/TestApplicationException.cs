using Volo.Abp;
using Volo.Abp.ExceptionHandling;

namespace LCH.Abp.BackgroundTasks.Activities;
public class TestApplicationException : AbpException, IHasHttpStatusCode
{
    public int HttpStatusCode { get; set; }
    public TestApplicationException(int httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
    }
}
