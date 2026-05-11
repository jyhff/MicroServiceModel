using Volo.Abp.Application.Dtos;

namespace LCH.Platform.Feedbacks;
public class FeedbackGetListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
    public string Category { get; set; }
    public FeedbackStatus? Status { get; set; }
}
