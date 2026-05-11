using Volo.Abp.Data;

namespace LCH.Abp.TaskManagement;

public abstract class BackgroundJobActionCreateOrUpdateDto
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 参数
    /// </summary>
    public ExtraPropertyDictionary Paramters { get; set; }

    public BackgroundJobActionCreateOrUpdateDto()
    {
        Paramters = new ExtraPropertyDictionary();
    }
}
