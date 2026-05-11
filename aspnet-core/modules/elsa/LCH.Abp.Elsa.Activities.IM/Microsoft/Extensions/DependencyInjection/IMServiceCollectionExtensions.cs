using Elsa.Options;
using LCH.Abp.Elsa.Activities.IM;

namespace Microsoft.Extensions.DependencyInjection;

public static class IMServiceCollectionExtensions
{
    public static ElsaOptionsBuilder AddIMActivities(this ElsaOptionsBuilder options)
    {
        options
            .AddActivity<SendMessage>();

        return options;
    }
}
