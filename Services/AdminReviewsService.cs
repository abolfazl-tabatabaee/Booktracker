using bookTracker.Common;
using bookTracker.Models.ViewModels;
using bookTracker.Repositories;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Services;

public sealed class AdminReviewsService : IAdminReviewsService
{
    private readonly IReviewRepository _reviews;

    public AdminReviewsService(IReviewRepository reviews) => _reviews = reviews;

    public async Task<AdminReviewsIndexVm> GetIndexAsync(string? q, int? rating, int? bookId, CancellationToken ct = default)
    {
        var rq = _reviews.QueryNoTracking().AsQueryable();

        if (bookId.HasValue)
            rq = rq.Where(r => r.BookId == bookId.Value);

        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
            rq = rq.Where(r => r.Rating == rating.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            rq = rq.Where(r => r.DisplayName.Contains(q) || r.Book.Title.Contains(q) || r.Text.Contains(q));
        }

        var items = await rq
            .OrderByDescending(r => r.CreatedAt)
            .Take(200)
            .Select(r => new AdminReviewListItemVm
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                DisplayName = r.DisplayName,
                UserId = r.UserId,
                Rating = r.Rating,
                TextPreview = r.Text.Length > 90 ? r.Text.Substring(0, 90) + "…" : r.Text,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);

        return new AdminReviewsIndexVm { Q = q, Rating = rating, BookId = bookId, Items = items };
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var r = await _reviews.GetByIdAsync(id, ct);
        if (r is null) return Result.NotFound();

        _reviews.Remove(r);
        await _reviews.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
