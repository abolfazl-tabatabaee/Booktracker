using bookTracker.Models.Entities;

namespace bookTracker.Repositories;

public interface IReviewRepository
{
    IQueryable<Review> QueryNoTracking();
    Task<Review?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    void Remove(Review review);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
