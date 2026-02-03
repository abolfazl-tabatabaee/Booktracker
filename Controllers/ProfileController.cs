using bookTracker.Common;
using bookTracker.Models.ViewModels;
using bookTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IProfileService _profile;

    public ProfileController(IProfileService profile)
    {
        _profile = profile;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var res = await _profile.GetProfileAsync(User, ct);

        if (res.Kind == ResultKind.Unauthorized)
            return RedirectToAction("Login", "Auth");

        if (res.Kind == ResultKind.NotFound)
            return NotFound();

        ViewBag.Success = TempData["Success"] as string;
        ViewBag.Error = TempData["Error"] as string;

        return View(res.Value!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(EditProfileVm input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "اطلاعات وارد شده معتبر نیست.";
            return RedirectToAction(nameof(Index));
        }

        var res = await _profile.UpdateProfileAsync(User, input, ct);
        if (res.Kind == ResultKind.Unauthorized)
            return RedirectToAction("Login", "Auth");

        if (!res.Success)
        {
            TempData["Error"] = res.Error ?? "خطا در ذخیره اطلاعات پروفایل.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "پروفایل بروزرسانی شد ✅";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordVm input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "ورودی‌های تغییر رمز معتبر نیست.";
            return RedirectToAction(nameof(Index));
        }

        var res = await _profile.ChangePasswordAsync(User, input, ct);
        if (res.Kind == ResultKind.Unauthorized)
            return RedirectToAction("Login", "Auth");

        if (!res.Success)
        {
            TempData["Error"] = res.Error ?? "خطا در تغییر رمز.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "رمز عبور با موفقیت تغییر کرد ✅";
        return RedirectToAction(nameof(Index));
    }
}




/*using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using bookTracker.Data;
using bookTracker.Models.Entities;
using bookTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bookTracker.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public ProfileController(AppDbContext db, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToAction("Login", "Auth");

        // بخش 1: اطلاعات کاربر (بدون Tracking)
        var u = await _userManager.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.FirstName,
                x.LastName,
                x.Email,
                x.UserName
            })
            .FirstAsync();

        var fullName = $"{(u.FirstName ?? "").Trim()} {(u.LastName ?? "").Trim()}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            fullName = User.FindFirst("FullName")?.Value ?? u.UserName ?? u.Email ?? "کاربر";

        // بخش 2: آمار (کاملاً روی SQL)
        var reviewsQ = _db.Reviews.AsNoTracking().Where(r => r.UserId == userId);

        var totalReviews = await reviewsQ.CountAsync();
        var avgRating = totalReviews == 0
            ? 0
            : (await reviewsQ.Select(r => (double?)r.Rating).AverageAsync() ?? 0);

        var distinctBooks = totalReviews == 0
            ? 0
            : await reviewsQ.Select(r => r.BookId).Distinct().CountAsync();

        var lastActivity = totalReviews == 0
            ? null
            : await reviewsQ.MaxAsync(r => (DateTime?)r.CreatedAt);

        // بخش 3: آخرین 5 فعالیت
        var recent = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId)
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
            .ToListAsync();

        var vm = new ProfileVm
        {
            FullName = fullName,
            Email = u.Email ?? u.UserName ?? "",
            RoleName = "User", // پروژه‌ات فقط همین رول رو داره

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

        // پیام‌های TempData (برای نمایش در View)
        ViewBag.Success = TempData["Success"] as string;
        ViewBag.Error = TempData["Error"] as string;

        return View(vm);
    }

    // بخش 4 - عملیات: ویرایش نام/فامیل
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(EditProfileVm input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "اطلاعات وارد شده معتبر نیست.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Auth");

        user.FirstName = input.FirstName?.Trim();
        user.LastName = input.LastName?.Trim();

        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded)
        {
            TempData["Error"] = "خطا در ذخیره اطلاعات پروفایل.";
            return RedirectToAction(nameof(Index));
        }

        // برای آپدیت شدن Claim FullName در هدر
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "پروفایل بروزرسانی شد ✅";
        return RedirectToAction(nameof(Index));
    }

    // بخش 4 - عملیات: تغییر رمز عبور
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordVm input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "ورودی‌های تغییر رمز معتبر نیست.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Auth");

        var res = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        if (!res.Succeeded)
        {
            // خطای رایج: رمز فعلی اشتباه است
            TempData["Error"] = res.Errors.FirstOrDefault()?.Description ?? "خطا در تغییر رمز.";
            return RedirectToAction(nameof(Index));
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "رمز عبور با موفقیت تغییر کرد ✅";
        return RedirectToAction(nameof(Index));
    }
}
*/