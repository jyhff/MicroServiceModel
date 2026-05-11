using System;

namespace LCH.Abp.DataProtection;

public interface IJavaScriptTypeConvert
{
    JavaScriptTypeConvertResult Convert(Type propertyType);
}
