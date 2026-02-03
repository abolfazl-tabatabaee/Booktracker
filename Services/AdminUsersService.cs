using bookTracker.Common;
using bookTracker.Models.Entities;
using bookTracker.Models.ViewModels;
using bookTracker.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Services;

public sealed class AdminUsersService : IAdminUsersService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IReviewRepository _reviews;

    public AdminUsersService(UserManager<AppUser> userManager, IReviewRepository reviews)
    {
        _userManager = userManager;
        _reviews = reviews;
    }

    public async Task<AdminUsersIndexVm> GetIndexAsync(string? q, CancellationToken ct = default)
    {
        var uq = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            uq = uq.Where(u =>
                (u.FirstName ?? "").Contains(q) ||
                (u.LastName ?? "").Contains(q) ||
                (u.Email ?? "").Contains(q) ||
                (u.UserName ?? "").Contains(q));
        }

        // آمار Reviewها برای هر کاربر
        var stats = await _reviews.QueryNoTracking()
            .Where(r => r.UserId != null)
            .GroupBy(r => r.UserId!)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count(),
                Avg = g.Average(x => (double)x.Rating)
            })
            .ToListAsync(ct);

        var statsDict = stats.ToDictionary(x => x.UserId, x => (x.Count, x.Avg));

        var users = await uq
            .OrderByDescending(u => u.Id)
            .Take(200)
            .ToListAsync(ct);

        var items = new List<AdminUserListItemVm>();

        foreach (var u in users)
        {
            var fullName = $"{(u.FirstName ?? "").Trim()} {(u.LastName ?? "").Trim()}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = u.UserName ?? u.Email ?? "—";

            var isAdmin = await _userManager.IsInRoleAsync(u, "Admin");

            var rc = 0;
            var avg = 0d;

            if (!string.IsNullOrWhiteSpace(u.Id) && statsDict.TryGetValue(u.Id, out var st))
            {
                rc = st.Count;
                avg = st.Avg;
            }

            items.Add(new AdminUserListItemVm
            {
                Id = u.Id,
                FullName = fullName,
                Email = u.Email ?? u.UserName ?? "",
                IsAdmin = isAdmin,
                ReviewCount = rc,
                AvgRating = avg
            });
        }

        return new AdminUsersIndexVm { Q = q, Items = items };
    }

    public async Task<Result> ToggleAdminAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return Result.NotFound();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        IdentityResult res;
        if (isAdmin)
            res = await _userManager.RemoveFromRoleAsync(user, "Admin");
        else
            res = await _userManager.AddToRoleAsync(user, "Admin");

        if (!res.Succeeded)
        {
            var err = string.Join(" | ", res.Errors.Select(e => e.Description));
            return Result.Fail(err);
        }

        return Result.Ok();
    }
}
