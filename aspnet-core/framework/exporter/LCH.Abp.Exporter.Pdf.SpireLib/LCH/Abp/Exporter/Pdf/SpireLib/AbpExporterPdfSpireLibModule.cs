using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LCH.Abp.Exporter.Pdf.SpireLib;

[DependsOn(typeof(AbpExporterPdfModule))]
public class AbpExporterPdfSpireLibModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IExcelToPdfProvider, SpireExcelToPdfProvider>();
    }
}
