using bookTracker.Common;
using bookTracker.Models.Entities;
using bookTracker.Models.ViewModels;
using bookTracker.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IReviewRepository _reviews;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public ProfileService(
        IReviewRepository reviews,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _reviews = reviews;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<ProfileVm>> GetProfileAsync(
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken ct = default)
    {
        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
            return Result<ProfileVm>.Unauthorized();

        var u = await _userManager.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.UserName
            })
            .FirstOrDefaultAsync(ct);

        if (u is null)
            return Result<ProfileVm>.NotFound();

        var fullName = $"{(u.FirstName ?? "").Trim()} {(u.LastName ?? "").Trim()}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            fullName = user.FindFirst("FullName")?.Value ?? u.UserName ?? u.Email ?? "کاربر";

        // ✅ نقش واقعی
        var roleName = "User";
        var userEntity = await _userManager.FindByIdAsync(u.Id);
        if (userEntity is not null)
        {
            var roles = await _userManager.GetRolesAsync(userEntity);
            // اگر چند نقش داری: اولویت Admin بعد User
            if (roles.Contains("Admin")) roleName = "Admin";
            else if (roles.Count > 0) roleName = roles[0];
        }

        var reviewsQ = _reviews.QueryNoTracking().Where(r => r.UserId == userId);

        var totalReviews = await reviewsQ.CountAsync(ct);

        var avgRating = totalReviews == 0
            ? 0
            : (await reviewsQ.Select(r => (double?)r.Rating).AverageAsync(ct) ?? 0);

        var distinctBooks = totalReviews == 0
            ? 0
            : await reviewsQ.Select(r => r.BookId).Distinct().CountAsync(ct);

        var lastActivity = totalReviews == 0
            ? null
            : await reviewsQ.MaxAsync(r => (DateTime?)r.CreatedAt, ct);

        var recent = await reviewsQ
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new RecentReviewVm
            {
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                Rating = r.Rating,
                TextPreview = r.Text.Length > 80 ? r.Text.Substring(0, 80) + "…" : r.Text,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);

        var vm = new ProfileVm
        {
            FullName = fullName,
            Email = u.Email ?? u.UserName ?? "",
            RoleName = roleName,

            Stats = new ProfileStatsVm
            {
                TotalReviews = totalReviews,
                AvgRatingGiven = avgRating,
                DistinctBooksReviewed = distinctBooks,
                LastActivityUtc = lastActivity
            },

            RecentReviews = recent,

            EditProfile = new EditProfileVm
            {
                FirstName = (u.FirstName ?? "").Trim(),
                LastName = (u.LastName ?? "").Trim()
            },

            ChangePassword = new ChangePasswordVm()
        };

        return Result<ProfileVm>.Ok(vm);
    }

    public async Task<Result> UpdateProfileAsync(
        System.Security.Claims.ClaimsPrincipal user,
        EditProfileVm input,
        CancellationToken ct = default)
    {
        var u = await _userManager.GetUserAsync(user);
        if (u is null) return Result.Unauthorized();

        u.FirstName = input.FirstName?.Trim();
        u.LastName = input.LastName?.Trim();

        var res = await _userManager.UpdateAsync(u);
        if (!res.Succeeded)
            return Result.Fail("خطا در ذخیره اطلاعات پروفایل.");

        await _signInManager.RefreshSignInAsync(u);
        return Result.Ok();
    }

    public async Task<Result> ChangePasswordAsync(
        System.Security.Claims.ClaimsPrincipal user,
        ChangePasswordVm input,
        CancellationToken ct = default)
    {
        var u = await _userManager.GetUserAsync(user);
        if (u is null) return Result.Unauthorized();

        var res = await _userManager.ChangePasswordAsync(u, input.CurrentPassword, input.NewPassword);
        if (!res.Succeeded)
        {
            var err = res.Errors.FirstOrDefault()?.Description ?? "خطا در تغییر رمز.";
            return Result.Validation(new Dictionary<string, string[]> { ["ChangePassword"] = [err] }, err);
        }

        await _signInManager.RefreshSignInAsync(u);
        return Result.Ok();
    }
}
