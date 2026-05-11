using Volo.Abp.Domain.Entities;

namespace LCH.Platform.Feedbacks;
public class FeedbackCommentUpdateDto : FeedbackCommentCreateOrUpdateDto, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}
