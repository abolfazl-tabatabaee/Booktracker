using bookTracker.Data;
using bookTracker.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly AppDbContext _db;

    public BookRepository(AppDbContext db) => _db = db;

    public IQueryable<Book> QueryNoTracking()
        => _db.Books.AsNoTracking();

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => _db.Books.AsNoTracking().AnyAsync(b => b.Id == id, ct);

    public Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<Book?> GetByIdWithReviewsNoTrackingAsync(int id, CancellationToken ct = default)
        => _db.Books
            .AsNoTracking()
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task AddAsync(Book book, CancellationToken ct = default)
        => _db.Books.AddAsync(book, ct).AsTask();

    public void Remove(Book book) => _db.Books.Remove(book);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
