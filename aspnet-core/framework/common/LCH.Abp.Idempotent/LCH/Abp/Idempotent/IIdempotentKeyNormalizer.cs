using Volo.Abp.DynamicProxy;

namespace LCH.Abp.Idempotent;

public interface IIdempotentKeyNormalizer
{
    string NormalizeKey(IdempotentKeyNormalizerContext context);
}
