using bookTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminDashboardService _dashboard;

    public AdminController(IAdminDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "پنل ادمین";
        ViewData["Page"] = "admin";

        var vm = await _dashboard.GetDashboardAsync(ct);
        return View(vm);
    }
}


