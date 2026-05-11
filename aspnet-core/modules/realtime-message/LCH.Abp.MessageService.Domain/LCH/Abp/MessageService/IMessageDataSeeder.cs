using System;
using System.Threading.Tasks;

namespace LCH.Abp.MessageService;

public interface IMessageDataSeeder
{
    Task SeedAsync(Guid? tenantId = null);
}
