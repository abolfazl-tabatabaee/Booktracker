namespace bookTracker.Models.ViewModels;

public class ProfileVm
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string RoleName { get; set; } = "User";

    public ProfileStatsVm Stats { get; set; } = new();

    public List<RecentReviewVm> RecentReviews { get; set; } = new();

    public EditProfileVm EditProfile { get; set; } = new();
    public ChangePasswordVm ChangePassword { get; set; } = new();
}

public class ProfileStatsVm
{
    public int TotalReviews { get; set; }
    public double AvgRatingGiven { get; set; }
    public int DistinctBooksReviewed { get; set; }
    public DateTime? LastActivityUtc { get; set; }
}

public class RecentReviewVm
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = "";
    public int Rating { get; set; }
    public string TextPreview { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
