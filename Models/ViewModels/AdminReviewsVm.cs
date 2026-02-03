namespace bookTracker.Models.ViewModels;

public class AdminReviewListItemVm
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? UserId { get; set; }
    public int Rating { get; set; }
    public string TextPreview { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class AdminReviewsIndexVm
{
    public string? Q { get; set; }
    public int? Rating { get; set; }
    public int? BookId { get; set; }
    public List<AdminReviewListItemVm> Items { get; set; } = new();
}
