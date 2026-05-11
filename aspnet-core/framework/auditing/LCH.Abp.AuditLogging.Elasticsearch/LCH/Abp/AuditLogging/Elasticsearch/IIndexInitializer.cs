using System.Threading.Tasks;

namespace LCH.Abp.AuditLogging.Elasticsearch;

public interface IIndexInitializer
{
    Task InitializeAsync();
}
