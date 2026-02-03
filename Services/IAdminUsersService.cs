using bookTracker.Common;
using bookTracker.Models.ViewModels;

namespace bookTracker.Services;

public interface IAdminUsersService
{
    Task<AdminUsersIndexVm> GetIndexAsync(string? q, CancellationToken ct = default);
    Task<Result> ToggleAdminAsync(string id, CancellationToken ct = default);
}
