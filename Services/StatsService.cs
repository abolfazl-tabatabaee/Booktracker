using bookTracker.Repositories;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Services;

public sealed class StatsService : IStatsService
{
    private readonly IBookRepository _books;
    private readonly IReviewRepository _reviews;

    public StatsService(IBookRepository books, IReviewRepository reviews)
    {
        _books = books;
        _reviews = reviews;
    }

    public async Task<(int totalBooks, int totalReviews, int genres)> GetAsync(CancellationToken ct = default)
    {
        var totalBooks = await _books.QueryNoTracking().CountAsync(ct);
        var totalReviews = await _reviews.QueryNoTracking().CountAsync(ct);

        var genres = await _books.QueryNoTracking()
            .Select(b => (b.Genre ?? "").Trim())
            .Where(g => g != "")
            .Distinct()
            .CountAsync(ct);

        return (totalBooks, totalReviews, genres);
    }
}
