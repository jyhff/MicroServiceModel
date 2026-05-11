using System;

namespace LCH.Abp.Webhooks.Saas;

[Serializable]
public class TenantWto
{
    public Guid Id { get; set; }

    public string Name { get; set; }
}

