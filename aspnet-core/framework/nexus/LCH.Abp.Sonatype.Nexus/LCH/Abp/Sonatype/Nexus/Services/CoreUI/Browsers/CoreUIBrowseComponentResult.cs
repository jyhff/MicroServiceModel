using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LCH.Abp.Sonatype.Nexus.Services.CoreUI.Browsers;

[Serializable]
public class CoreUIBrowseComponentResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public List<CoreUIBrowseComponent> Data { get; set; }

    public CoreUIBrowseComponentResult()
    {
        Data = new List<CoreUIBrowseComponent>();
    }
}
