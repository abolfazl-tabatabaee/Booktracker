namespace bookTracker.Services;

public interface IStatsService
{
    Task<(int totalBooks, int totalReviews, int genres)> GetAsync(CancellationToken ct = default);
}
