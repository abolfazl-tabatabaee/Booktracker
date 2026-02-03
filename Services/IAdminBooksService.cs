using bookTracker.Common;
using bookTracker.Models.ViewModels;

namespace bookTracker.Services;

public interface IAdminBooksService
{
    Task<AdminBooksIndexVm> GetIndexAsync(string? q, CancellationToken ct = default);
    Task<AdminBookEditVm?> GetEditVmAsync(int id, CancellationToken ct = default);
    Task<Result> CreateAsync(AdminBookEditVm input, CancellationToken ct = default);
    Task<Result> UpdateAsync(AdminBookEditVm input, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
