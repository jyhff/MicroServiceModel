using LCH.Abp.Elasticsearch.Jobs.Localization;
using Volo.Abp.Localization;

namespace LCH.Abp.Elasticsearch.Jobs;

internal static class LocalizableStatic
{
    public static ILocalizableString Create(string name)
    {
        return LocalizableString.Create<ElasticsearchJobsResource>(name);
    }
}
