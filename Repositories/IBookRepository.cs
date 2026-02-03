using bookTracker.Models.Entities;

namespace bookTracker.Repositories;

public interface IBookRepository
{
    IQueryable<Book> QueryNoTracking();
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    /// <summary>Tracked entity (برای Update/Delete)</summary>
    Task<Book?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>بدون Tracking + همراه Reviews (برای Details API)</summary>
    Task<Book?> GetByIdWithReviewsNoTrackingAsync(int id, CancellationToken ct = default);

    Task AddAsync(Book book, CancellationToken ct = default);
    void Remove(Book book);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
