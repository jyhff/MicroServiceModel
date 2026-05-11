using System;

namespace LCH.Abp.DataProtection;

public interface IDataProtected
{
    Guid? CreatorId { get; }
}