using bookTracker.Common;
using bookTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly IAdminUsersService _users;

    public AdminUsersController(IAdminUsersService users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, CancellationToken ct)
    {
        ViewData["Title"] = "مدیریت کاربران";
        ViewData["Page"] = "admin";

        var vm = await _users.GetIndexAsync(q, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(string id, CancellationToken ct)
    {
        var res = await _users.ToggleAdminAsync(id, ct);
        if (res.Kind == ResultKind.NotFound) return NotFound();

        return RedirectToAction(nameof(Index));
    }
}

