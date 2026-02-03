namespace bookTracker.Models.ViewModels;

public class AdminUserListItemVm
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsAdmin { get; set; }
    public int ReviewCount { get; set; }
    public double AvgRating { get; set; }
}

public class AdminUsersIndexVm
{
    public string? Q { get; set; }
    public List<AdminUserListItemVm> Items { get; set; } = new();
}
