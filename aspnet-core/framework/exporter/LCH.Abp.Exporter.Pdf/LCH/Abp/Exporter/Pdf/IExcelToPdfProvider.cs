using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LCH.Abp.Exporter.Pdf;

public interface IExcelToPdfProvider
{
    Task<Stream> ParseAsync(Stream excelStream, CancellationToken cancellationToken = default);
}
