using System;

namespace LCH.Abp.Wrapper;

public interface IExceptionWrapHandlerFactory
{
    IExceptionWrapHandler CreateFor(ExceptionWrapContext context);
}
