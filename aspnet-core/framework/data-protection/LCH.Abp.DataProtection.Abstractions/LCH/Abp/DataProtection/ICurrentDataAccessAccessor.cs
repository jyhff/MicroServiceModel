namespace LCH.Abp.DataProtection;

public interface ICurrentDataAccessAccessor
{
    DataAccessOperation[] Current { get; set; }
}
