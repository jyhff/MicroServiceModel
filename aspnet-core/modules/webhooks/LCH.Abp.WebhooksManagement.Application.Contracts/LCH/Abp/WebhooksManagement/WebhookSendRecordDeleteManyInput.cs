using System;
using System.Collections.Generic;

namespace LCH.Abp.WebhooksManagement;
public class WebhookSendRecordDeleteManyInput
{
    public List<Guid> RecordIds { get; set; } = new List<Guid>();
}
