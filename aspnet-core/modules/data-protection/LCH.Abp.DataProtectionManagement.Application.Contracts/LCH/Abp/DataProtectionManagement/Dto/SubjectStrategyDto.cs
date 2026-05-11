using LCH.Abp.DataProtection;

namespace LCH.Abp.DataProtectionManagement;

public class SubjectStrategyDto
{
    public bool IsEnabled { get; set; }
    public string SubjectName { get; set; }
    public string SubjectId { get; set; }
    public DataAccessStrategy Strategy { get; set; }
}
