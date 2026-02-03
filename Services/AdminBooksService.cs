using bookTracker.Common;
using bookTracker.Models.Entities;
using bookTracker.Models.ViewModels;
using bookTracker.Repositories;
using bookTracker.Storage;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Services;

public sealed class AdminBooksService : IAdminBooksService
{
    private readonly IBookRepository _books;
    private readonly ICoverStorage _covers;

    public AdminBooksService(IBookRepository books, ICoverStorage covers)
    {
        _books = books;
        _covers = covers;
    }

    public async Task<AdminBooksIndexVm> GetIndexAsync(string? q, CancellationToken ct = default)
    {
        var booksQ = _books.QueryNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            booksQ = booksQ.Where(b =>
                b.Title.Contains(q) ||
                b.Author.Contains(q) ||
                b.Genre.Contains(q));
        }

        var items = await booksQ
            .OrderByDescending(b => b.Id)
            .Select(b => new AdminBookListItemVm
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Year = b.Year,
                Genre = b.Genre,
                ReviewCount = b.Reviews.Count,
                AvgRating = b.Reviews.Count == 0 ? 0 : b.Reviews.Average(r => (double)r.Rating)
            })
            .ToListAsync(ct);

        return new AdminBooksIndexVm { Q = q, Items = items };
    }

    public async Task<AdminBookEditVm?> GetEditVmAsync(int id, CancellationToken ct = default)
    {
        var b = await _books.QueryNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return null;

        return new AdminBookEditVm
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            Genre = b.Genre,
            Year = b.Year,
            Lang = b.Lang,
            CoverPath = b.CoverPath,
            Description = b.Description,
            TagsCsv = b.TagsCsv
        };
    }

    public async Task<Result> CreateAsync(AdminBookEditVm input, CancellationToken ct = default)
    {
        string? coverPath = input.CoverPath;

        if (input.CoverFile is not null && input.CoverFile.Length > 0)
        {
            var save = await _covers.SaveCoverAsync(input.CoverFile, ct);
            if (!save.Success)
                return Result.Validation(
                    save.ValidationErrors?.ToDictionary(k => k.Key, v => v.Value)
                    ?? new Dictionary<string, string[]> { ["CoverFile"] = ["خطا در آپلود کاور"] }
                );

            coverPath = save.Value;
        }

        var book = new Book
        {
            Title = input.Title.Trim(),
            Author = input.Author.Trim(),
            Genre = input.Genre.Trim(),
            Year = input.Year,
            Lang = (input.Lang ?? "fa").Trim(),
            CoverPath = string.IsNullOrWhiteSpace(coverPath) ? null : coverPath.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            TagsCsv = string.IsNullOrWhiteSpace(input.TagsCsv) ? null : input.TagsCsv.Trim()
        };

        await _books.AddAsync(book, ct);
        await _books.SaveChangesAsync(ct);

        return Result.Ok();
    }

    public async Task<Result> UpdateAsync(AdminBookEditVm input, CancellationToken ct = default)
    {
        if (input.Id is null)
            return Result.Validation(new Dictionary<string, string[]> { ["Id"] = ["شناسه نامعتبر است."] });

        var b = await _books.GetByIdAsync(input.Id.Value, ct);
        if (b is null) return Result.NotFound();

        if (input.CoverFile is not null && input.CoverFile.Length > 0)
        {
            var save = await _covers.SaveCoverAsync(input.CoverFile, ct);
            if (!save.Success)
                return Result.Validation(
                    save.ValidationErrors?.ToDictionary(k => k.Key, v => v.Value)
                    ?? new Dictionary<string, string[]> { ["CoverFile"] = ["خطا در آپلود کاور"] }
                );

            _covers.DeleteIfLocal(b.CoverPath);
            b.CoverPath = save.Value;
        }

        b.Title = input.Title.Trim();
        b.Author = input.Author.Trim();
        b.Genre = input.Genre.Trim();
        b.Year = input.Year;
        b.Lang = (input.Lang ?? "fa").Trim();
        b.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        b.TagsCsv = string.IsNullOrWhiteSpace(input.TagsCsv) ? null : input.TagsCsv.Trim();

        await _books.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var b = await _books.GetByIdAsync(id, ct);
        if (b is null) return Result.NotFound();

        _covers.DeleteIfLocal(b.CoverPath);

        _books.Remove(b);
        await _books.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
