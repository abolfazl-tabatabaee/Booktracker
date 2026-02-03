using bookTracker.Common;
using bookTracker.Models.ViewModels;
using bookTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace bookTracker.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBooksController : Controller
{
    private readonly IAdminBooksService _books;

    public AdminBooksController(IAdminBooksService books)
    {
        _books = books;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, CancellationToken ct)
    {
        ViewData["Title"] = "مدیریت کتاب‌ها";
        ViewData["Page"] = "admin";

        var vm = await _books.GetIndexAsync(q, ct);
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "افزودن کتاب";
        ViewData["Page"] = "admin";
        return View(new AdminBookEditVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminBookEditVm input, CancellationToken ct)
    {
        ViewData["Title"] = "افزودن کتاب";
        ViewData["Page"] = "admin";

        if (!ModelState.IsValid) return View(input);

        var res = await _books.CreateAsync(input, ct);

        if (!res.Success)
        {
            ApplyValidationErrorsToModelState(res);
            return View(input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "ویرایش کتاب";
        ViewData["Page"] = "admin";

        var vm = await _books.GetEditVmAsync(id, ct);
        if (vm is null) return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminBookEditVm input, CancellationToken ct)
    {
        ViewData["Title"] = "ویرایش کتاب";
        ViewData["Page"] = "admin";

        if (!ModelState.IsValid) return View(input);

        var res = await _books.UpdateAsync(input, ct);

        if (res.Kind == ResultKind.NotFound) return NotFound();

        if (!res.Success)
        {
            ApplyValidationErrorsToModelState(res);
            return View(input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var res = await _books.DeleteAsync(id, ct);
        if (res.Kind == ResultKind.NotFound) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private void ApplyValidationErrorsToModelState(Result res)
    {
        if (res.ValidationErrors is null || res.ValidationErrors.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(res.Error))
                ModelState.AddModelError("", res.Error);
            return;
        }

        foreach (var (key, messages) in res.ValidationErrors)
        {
            foreach (var msg in messages)
                ModelState.AddModelError(key, msg);
        }
    }
}




