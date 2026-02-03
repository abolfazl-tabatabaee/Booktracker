using bookTracker.Common;
using bookTracker.Models.ViewModels;

namespace bookTracker.Services;

public interface IAdminReviewsService
{
    Task<AdminReviewsIndexVm> GetIndexAsync(string? q, int? rating, int? bookId, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
