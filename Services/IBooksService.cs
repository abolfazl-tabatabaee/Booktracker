using System.Security.Claims;
using bookTracker.Common;
using bookTracker.Models.DTOs;

namespace bookTracker.Services;

public interface IBooksService
{
    Task<List<BookListDto>> GetAllAsync(CancellationToken ct = default);
    Task<BookDetailsDto?> GetOneAsync(int id, CancellationToken ct = default);
    Task<Result> AddReviewAsync(int bookId, ClaimsPrincipal user, CreateReviewDto input, CancellationToken ct = default);
}
