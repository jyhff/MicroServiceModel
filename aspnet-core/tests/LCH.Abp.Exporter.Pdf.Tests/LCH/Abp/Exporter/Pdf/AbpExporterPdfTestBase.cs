using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace LCH.Abp.Exporter.Pdf;
public abstract class AbpExporterPdfTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
        where TStartupModule : IAbpModule
{
}
