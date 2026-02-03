using bookTracker.Data;
using bookTracker.Models.Entities;
using bookTracker.Repositories;
using bookTracker.Security;
using bookTracker.Services;
using bookTracker.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSetting(WebHostDefaults.PreventHostingStartupKey, "true");

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/Login";
});

// ✅ Anti-forgery: اسم کوکی رو ثابت می‌کنیم تا تو کنترلرها brittle نباشه
const string AntiForgeryCookieName = "bookTracker.AntiCsrf";

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = AntiForgeryCookieName;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    // اگر SPA داری و نیاز داری JS به کوکی دسترسی داشته باشه => HttpOnly=false
    // ولی در حالت MVC معمولاً لازم نیست. پس پیش‌فرض نگه می‌داریم.
    // options.Cookie.HttpOnly = false;
});

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

builder.Services.AddScoped<ICoverStorage, LocalCoverStorage>();

builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<IStatsService, StatsService>();

builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminBooksService, AdminBooksService>();
builder.Services.AddScoped<IAdminReviewsService, AdminReviewsService>();
builder.Services.AddScoped<IAdminUsersService, AdminUsersService>();

builder.Services.AddScoped<IProfileService, ProfileService>();

var app = builder.Build();

// ✅ Security headers + CSP
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

    // CSP: در Dev کمی permissive تر، در Prod سخت‌تر
    var isDev = app.Environment.IsDevelopment();

    var csp =
        "default-src 'self'; " +
        "img-src 'self' data:; " +
        (isDev
            ? "style-src 'self' 'unsafe-inline'; "
            : "style-src 'self'; ") +
        "script-src 'self'; " +
        "font-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'none'";

    ctx.Response.Headers["Content-Security-Policy"] = csp;

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Seed roles/users فقط در Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    const string userRole = "User";
    const string adminRole = "Admin";

    if (!await roleManager.RoleExistsAsync(userRole))
        await roleManager.CreateAsync(new IdentityRole(userRole));

    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    var adminEmail = (app.Configuration["SeedAdmin:Email"] ?? "admin@booktracker.local")
        .Trim()
        .ToLowerInvariant();

    // ✅ پسورد فقط از Secret/Env بیاد. اگر ست نشده seed انجام نشه.
    var adminPass = (app.Configuration["SeedAdmin:Password"] ?? "").Trim();
    if (!string.IsNullOrWhiteSpace(adminPass))
    {
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = app.Configuration["SeedAdmin:FirstName"] ?? "ادمین",
                LastName = app.Configuration["SeedAdmin:LastName"] ?? "سیستم",
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(admin, adminPass);
            if (!create.Succeeded)
            {
                var errors = string.Join(" | ", create.Errors.Select(e => e.Description));
                Console.WriteLine("Seed admin failed: " + errors);
            }
        }

        if (admin is not null)
        {
            if (!await userManager.IsInRoleAsync(admin, adminRole))
                await userManager.AddToRoleAsync(admin, adminRole);

            if (!await userManager.IsInRoleAsync(admin, userRole))
                await userManager.AddToRoleAsync(admin, userRole);
        }
    }
    else
    {
        Console.WriteLine("Seed admin skipped: SeedAdmin:Password is not configured.");
    }
}

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
