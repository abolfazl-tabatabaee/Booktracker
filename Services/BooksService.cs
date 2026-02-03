using System.Security.Claims;
using bookTracker.Common;
using bookTracker.Models.DTOs;
using bookTracker.Models.Entities;
using bookTracker.Repositories;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Services;

public sealed class BooksService : IBooksService
{
    private readonly IBookRepository _books;
    private readonly IReviewRepository _reviews;

    public BooksService(IBookRepository books, IReviewRepository reviews)
    {
        _books = books;
        _reviews = reviews;
    }

    public async Task<List<BookListDto>> GetAllAsync(CancellationToken ct = default)
    {
        var raw = await _books.QueryNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Author,
                b.Year,
                b.Genre,
                b.Lang,
                b.TagsCsv,
                Cover = b.CoverPath ?? "",
                ReviewCount = b.Reviews.Count(),
                AvgRating = b.Reviews.Select(r => (double?)r.Rating).Average() ?? 0.0
            })
            .ToListAsync(ct);

        return raw.Select(b => new BookListDto(
            b.Id,
            b.Title,
            b.Author,
            b.Year,
            b.Genre,
            b.Lang,
            TagParser.
            SplitTags(b.TagsCsv),
            b.Cover,
            b.ReviewCount,
            b.AvgRating
        )).ToList();
    }

    public async Task<BookDetailsDto?> GetOneAsync(int id, CancellationToken ct = default)
    {
        var book = await _books.GetByIdWithReviewsNoTrackingAsync(id, ct);
        if (book is null) return null;

        return new BookDetailsDto(
            book.Id,
            book.Title,
            book.Author,
            book.Year,
            book.Genre,
            book.Lang,
            TagParser.SplitTags(book.TagsCsv),
            book.Description ?? "",
            book.CoverPath ?? "",
            book.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto(
                    r.Id,
                    r.DisplayName,
                    r.Rating,
                    r.Text,
                    new DateTimeOffset(r.CreatedAt).ToUnixTimeMilliseconds()
                ))
                .ToList()
        );
    }

    public async Task<Result> AddReviewAsync(int bookId, ClaimsPrincipal user, CreateReviewDto input, CancellationToken ct = default)
    {
        var exists = await _books.ExistsAsync(bookId, ct);
        if (!exists) return Result.NotFound();

        var text = (input.Text ?? "").Trim();
        if (text.Length < 10)
            return Result.Validation(new Dictionary<string, string[]> { ["Text"] = ["متن نظر باید حداقل 10 کاراکتر باشد."] });

        if (text.Length > 400)
            return Result.Validation(new Dictionary<string, string[]> { ["Text"] = ["متن نظر حداکثر 400 کاراکتر است."] });

        if (input.Rating < 1 || input.Rating > 5)
            return Result.Validation(new Dictionary<string, string[]> { ["Rating"] = ["امتیاز باید بین 1 تا 5 باشد."] });

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Unauthorized();

        var fullName = user.FindFirst("FullName")?.Value;
        var displayName = string.IsNullOrWhiteSpace(fullName)
            ? (user.Identity?.Name ?? "کاربر")
            : fullName;

        var review = new Review
        {
            BookId = bookId,
            DisplayName = displayName.Trim(),
            Rating = input.Rating,
            Text = text,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _reviews.AddAsync(review, ct);
        await _reviews.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
