using bookTracker.Common;
using bookTracker.Models.ViewModels;

namespace bookTracker.Services;

public interface IProfileService
{
    Task<Result<ProfileVm>> GetProfileAsync(System.Security.Claims.ClaimsPrincipal user, CancellationToken ct = default);
    Task<Result> UpdateProfileAsync(System.Security.Claims.ClaimsPrincipal user, EditProfileVm input, CancellationToken ct = default);
    Task<Result> ChangePasswordAsync(System.Security.Claims.ClaimsPrincipal user, ChangePasswordVm input, CancellationToken ct = default);
}
