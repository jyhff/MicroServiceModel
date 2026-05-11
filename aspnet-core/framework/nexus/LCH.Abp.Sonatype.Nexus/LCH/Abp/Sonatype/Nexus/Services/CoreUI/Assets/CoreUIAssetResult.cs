using System;
using System.Text.Json.Serialization;

namespace LCH.Abp.Sonatype.Nexus.Services.CoreUI.Assets;

[Serializable]
public class CoreUIAssetResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public CoreUIAssetData Data { get; set; }
}
