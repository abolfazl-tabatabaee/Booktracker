using bookTracker.Models.Entities;
using bookTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers;

public class AuthController : Controller
{
    private const string DefaultRole = "User";

    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signIn;

    public AuthController(UserManager<AppUser> users, SignInManager<AppUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterVm());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = vm.FirstName.Trim(),
            LastName = vm.LastName.Trim()
        };

        var create = await _users.CreateAsync(user, vm.Password);
        if (!create.Succeeded)
        {
            foreach (var e in create.Errors)
                ModelState.AddModelError("", e.Description);

            return View(vm);
        }

        await _users.AddToRoleAsync(user, DefaultRole);

        await _signIn.SignInAsync(user, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();

        var result = await _signIn.PasswordSignInAsync(
            userName: email,
            password: vm.Password,
            isPersistent: vm.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "حساب کاربری موقتاً به دلیل تلاش‌های ناموفق قفل شده است. چند دقیقه دیگر دوباره تلاش کنید.");
            return View(vm);
        }

        ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");
        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
