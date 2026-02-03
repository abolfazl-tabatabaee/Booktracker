using bookTracker.Models.Entities;
using bookTracker.Models.ViewModels;
using bookTracker.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IBookRepository _books;
    private readonly IReviewRepository _reviews;
    private readonly UserManager<AppUser> _users;

    public AdminDashboardService(IBookRepository books, IReviewRepository reviews, UserManager<AppUser> users)
    {
        _books = books;
        _reviews = reviews;
        _users = users;
    }

    public async Task<AdminDashboardVm> GetDashboardAsync(CancellationToken ct = default)
    {
        var totalBooks = await _books.QueryNoTracking().CountAsync(ct);
        var totalReviews = await _reviews.QueryNoTracking().CountAsync(ct);
        var totalUsers = await _users.Users.AsNoTracking().CountAsync(ct);

        var avgRating = totalReviews == 0
            ? 0
            : (await _reviews.QueryNoTracking().Select(r => (double?)r.Rating).AverageAsync(ct) ?? 0);

        var lastReviews = await _reviews.QueryNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new AdminRecentReviewVm
            {
                ReviewId = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                DisplayName = r.DisplayName,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);

        return new AdminDashboardVm
        {
            TotalBooks = totalBooks,
            TotalUsers = totalUsers,
            TotalReviews = totalReviews,
            AvgRating = avgRating,
            RecentReviews = lastReviews
        };
    }
}
