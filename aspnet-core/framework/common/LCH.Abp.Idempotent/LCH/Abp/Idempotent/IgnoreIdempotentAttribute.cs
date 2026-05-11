using System;

namespace LCH.Abp.Idempotent;

[AttributeUsage(AttributeTargets.Method)]
public class IgnoreIdempotentAttribute : Attribute
{
}
