using System.Threading.Tasks;
using Volo.Abp.Auditing;

namespace LCH.Abp.AuditLogging;

public interface IAuditLogInfoToAuditLogConverter
{
    Task<AuditLog> ConvertAsync(AuditLogInfo auditLogInfo);
}
