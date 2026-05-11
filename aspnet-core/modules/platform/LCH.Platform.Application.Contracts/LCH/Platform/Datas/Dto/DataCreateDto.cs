using System;
using System.ComponentModel;

namespace LCH.Platform.Datas;

public class DataCreateDto : DataCreateOrUpdateDto
{
    [DisplayName("DisplayName:ParentData")]
    public Guid? ParentId { get; set; }
}
