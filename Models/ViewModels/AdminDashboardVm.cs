namespace bookTracker.Models.ViewModels;

public class AdminDashboardVm
{
    public int TotalBooks { get; set; }
    public int TotalUsers { get; set; }
    public int TotalReviews { get; set; }
    public double AvgRating { get; set; }

    public List<AdminRecentReviewVm> RecentReviews { get; set; } = new();
}

public class AdminRecentReviewVm
{
    public int ReviewId { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
