using Elastic.Clients.Elasticsearch;

namespace LCH.Abp.Elasticsearch
{
    public interface IElasticsearchClientFactory
    {
        ElasticsearchClient Create();
    }
}
