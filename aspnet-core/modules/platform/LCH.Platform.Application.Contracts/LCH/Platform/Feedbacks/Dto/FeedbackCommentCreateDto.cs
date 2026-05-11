using System.ComponentModel.DataAnnotations;
using Volo.Abp.Validation;

namespace LCH.Platform.Feedbacks;
public class FeedbackCommentCreateDto : FeedbackCommentCreateOrUpdateDto
{
    [Required]
    [DynamicStringLength(typeof(FeedbackCommentConsts), nameof(FeedbackCommentConsts.MaxCapacityLength))]
    public string Capacity { get; set; }
}
