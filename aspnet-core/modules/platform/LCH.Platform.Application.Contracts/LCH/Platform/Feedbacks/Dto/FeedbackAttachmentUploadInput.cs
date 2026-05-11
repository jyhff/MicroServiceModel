using Volo.Abp.Auditing;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace LCH.Platform.Feedbacks;

public class FeedbackAttachmentUploadInput
{
    [DisableAuditing]
    [DisableValidation]
    public IRemoteStreamContent File { get; set; }
}
