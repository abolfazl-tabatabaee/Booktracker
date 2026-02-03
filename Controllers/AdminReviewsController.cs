using bookTracker.Common;
using bookTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers;

[Authorize(Roles = "Admin")]
public class AdminReviewsController : Controller
{
    private readonly IAdminReviewsService _reviews;

    public AdminReviewsController(IAdminReviewsService reviews) => _reviews = reviews;

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int? rating, int? bookId, CancellationToken ct)
    {
        ViewData["Title"] = "مدیریت نظرات";
        ViewData["Page"] = "admin";

        var vm = await _reviews.GetIndexAsync(q, rating, bookId, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var res = await _reviews.DeleteAsync(id, ct);
        if (res.Kind == ResultKind.NotFound) return NotFound();

        return RedirectToAction(nameof(Index));
    }
}



