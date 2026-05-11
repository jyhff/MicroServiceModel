using Volo.Abp;

namespace LCH.Abp.Wrapper
{
    public class FakeException: AbpException
    {
        public FakeException()
        {
        }

        public FakeException(string message)
            :   base(message)
        {
        }
    }
}
