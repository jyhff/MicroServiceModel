using System;
using System.Text.Json.Serialization;

namespace LCH.Abp.Sonatype.Nexus.Repositories;

[Serializable]
public abstract class NexusRepository
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("online")]
    public bool Online { get; set; }
}
