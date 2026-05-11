using System;
using System.Text.Json.Serialization;

namespace LCH.Abp.Sonatype.Nexus.Repositories.Raw;

[Serializable]
public class RawStorage
{
    [JsonPropertyName("blobStoreName")]
    public string BlobStoreName { get; set; }

    [JsonPropertyName("strictContentTypeValidation")]
    public bool StrictContentTypeValidation { get; set; }

    [JsonPropertyName("RawGroup")]
    public RawGroup Group { get; set; }
}
