using System;

namespace LCH.Abp.Webhooks.Saas;

[Serializable]
public class EditionWto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; }
}
