using System;
using Volo.Abp.Domain.Repositories;

namespace LCH.Abp.WebhooksManagement;

public interface IWebhookEventRecordRepository : IRepository<WebhookEventRecord, Guid>
{
}
