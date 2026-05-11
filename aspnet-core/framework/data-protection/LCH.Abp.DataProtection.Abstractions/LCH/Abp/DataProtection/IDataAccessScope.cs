using System;

namespace LCH.Abp.DataProtection;

public interface IDataAccessScope
{
    DataAccessOperation[] Operations { get; }
    IDisposable BeginScope(DataAccessOperation[] operations = null);
}
