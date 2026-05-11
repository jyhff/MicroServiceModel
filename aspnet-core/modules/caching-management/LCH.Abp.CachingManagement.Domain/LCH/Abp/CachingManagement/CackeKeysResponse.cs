using System.Collections.Generic;

namespace LCH.Abp.CachingManagement;

public class CackeKeysResponse
{
    public string NextMarker { get; }

    public IEnumerable<string> Keys { get; }

    public CackeKeysResponse(
        string nextMarker,
        IEnumerable<string> keys)
    {
        NextMarker = nextMarker;
        Keys = keys;
    }
}
