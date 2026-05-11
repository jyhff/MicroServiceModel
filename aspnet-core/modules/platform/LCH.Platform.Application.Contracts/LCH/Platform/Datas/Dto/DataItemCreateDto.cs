using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Validation;

namespace LCH.Platform.Datas;

public class DataItemCreateDto : DataItemCreateOrUpdateDto
{
    [Required]
    [DynamicStringLength(typeof(DataItemConsts), nameof(DataItemConsts.MaxNameLength))]
    public string Name { get; set; }
}
