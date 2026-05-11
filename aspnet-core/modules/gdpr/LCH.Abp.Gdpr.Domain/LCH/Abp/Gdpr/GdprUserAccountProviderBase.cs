using System.Threading.Tasks;

namespace LCH.Abp.Gdpr;

public abstract class GdprUserAccountProviderBase : IGdprUserAccountProvider
{
    public abstract string Name { get; }

    public abstract Task DeleteAsync(GdprDeleteUserAccountContext context);
}
