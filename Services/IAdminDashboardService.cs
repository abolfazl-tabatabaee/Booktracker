using bookTracker.Models.ViewModels;

namespace bookTracker.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardVm> GetDashboardAsync(CancellationToken ct = default);
}
