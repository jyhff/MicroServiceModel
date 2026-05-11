using System.Collections.Generic;

namespace LCH.Abp.CachingManagement;

public class CacheKeysDto
{
    public string NextMarker { get; set; }

    public List<string> Keys { get; set; } = new List<string>();
}
