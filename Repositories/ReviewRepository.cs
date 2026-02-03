using bookTracker.Data;
using bookTracker.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _db;

    public ReviewRepository(AppDbContext db) => _db = db;

    public IQueryable<Review> QueryNoTracking()
        => _db.Reviews.AsNoTracking();

    public Task<Review?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task AddAsync(Review review, CancellationToken ct = default)
        => _db.Reviews.AddAsync(review, ct).AsTask();

    public void Remove(Review review) => _db.Reviews.Remove(review);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
