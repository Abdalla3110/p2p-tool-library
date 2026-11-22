using DAL.Data;
using DAL.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace PL.Controllers;

public class HomeController : Controller
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IAuthService _auth;
    private readonly IToolService _tools;
    private readonly ICategoryService _categories;
    private readonly IZoneService _zones;
    private readonly IUserService _users;
    private readonly IReportService _reports;
    private readonly IMemoryCache _cache;

    public HomeController(IAuthService auth, IToolService tools, ICategoryService categories,
        IZoneService zones, IUserService users, IReportService reports, IMemoryCache cache)
    {
        _auth = auth;
        _tools = tools;
        _categories = categories;
        _zones = zones;
        _users = users;
        _reports = reports;
        _cache = cache;
    }

public async Task<IActionResult> Index()
    {
        var tools = await _tools.GetFeaturedAsync();

        ViewBag.FeaturedTools = tools.Select(ToolCardViewModel.FromTool).ToList();
        ViewBag.Categories = await _categories.GetAllAsync();
        var stats = await _reports.GetHomepageStatsAsync();
        ViewBag.TotalTools = stats.TotalTools;
        ViewBag.TotalUsers = stats.TotalUsers;
        ViewBag.TotalBookings = stats.TotalBookings;
        return View();
    }

public IActionResult Privacy() => View();

public IActionResult Terms() => View();

public IActionResult Support() => View();

public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }

public async Task<IActionResult> Availability(int id, int? month)
    {
        var tool = await _tools.GetAnyToolWithCategoryAsync(id);
        if (tool is null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var ym = month ?? today.Year * 12 + today.Month - 1;
        var year = ym / 12;
        var mon = ym % 12 + 1;
        var daysInMonth = DateTime.DaysInMonth(year, mon);

        var avail = await _tools.GetMonthAvailabilityAsync(id, year, mon);

        var days = Enumerable.Range(1, daysInMonth)
            .Select(d => new { Day = new DateOnly(year, mon, d), Status = avail.GetValueOrDefault(new DateOnly(year, mon, d)) })
            .ToList();

        ViewBag.Tool = tool;
        ViewBag.Year = year;
        ViewBag.Month = mon;
        ViewBag.Days = days;
        ViewBag.PrevMonth = ym - 1;
        ViewBag.NextMonth = ym + 1;
        ViewBag.MonthParam = ym;
        return View();
    }

public async Task<IActionResult> Tools(BrowseToolsViewModel f)
    {
        var query = await _tools.GetBrowseQueryAsync(f);

        const int pageSize = 12;
        var total = await query.CountAsync();
        var tools = await query.Skip((f.Page - 1) * pageSize).Take(pageSize).ToListAsync();

        f.Tools = tools.Select(ToolCardViewModel.FromTool).ToList();
        f.TotalCount = total;
        f.PageSize = pageSize;
        f.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        f.Categories = await _categories.GetAllAsync();
        f.Zones = await _zones.GetAllAsync();
        return View(f);
    }

public async Task<IActionResult> Details(int id)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOwner = uid is not null && int.Parse(uid) == await _tools.GetOwnerUserIdAsync(id);
        var vm = await _tools.GetDetailsViewModelAsync(id, isOwner);
        if (vm is null) return NotFound();
        return View(vm);
    }

public async Task<IActionResult> PublicProfile(int id)
    {
        var user = await _users.GetPublicProfileAsync(id);
        if (user is null) return NotFound();

        var tools = await _tools.GetPublicToolsByOwnerAsync(id);

        ViewBag.User = user;
        ViewBag.Tools = tools.Select(ToolCardViewModel.FromTool).ToList();
        return View();
    }

[HttpGet]
public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

[EnableRateLimiting("auth")]
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (IsLockedOut(model.Email))
        {
            ModelState.AddModelError(string.Empty, "Too many failed attempts. Please try again in 15 minutes.");
            return View(model);
        }

        var user = await _auth.ValidateCredentialsAsync(model.Email, model.Password);
        if (user is null)
        {
            RecordFailure(model.Email);
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        ClearAttempts(model.Email);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "Member"),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        await _auth.TouchLastActiveAsync(user.UserId);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        // role-based landing
        var roleName = user.Role?.Name ?? "Member";
        return roleName switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "Librarian" => RedirectToAction("Index", "Library"),
            "Technician" => RedirectToAction("Index", "Maintenance"),
            _ => RedirectToAction("Index", "Member"),
        };
    }

[HttpGet]
public IActionResult Register() => View(new RegisterViewModel());

[EnableRateLimiting("auth")]
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var error = await _auth.RegisterAsync(
            model.FullName, model.Phone, model.Email, model.Password, model.ReferralCode);

        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        // DEV-ONLY: surface the verification link until SMTP is configured
        var newUser = await _auth.FindByEmailAsync(model.Email);
        if (newUser?.EmailVerificationToken is not null)
            TempData["DevVerifyLink"] = Url.Action("VerifyEmail", "Home",
                new { token = newUser.EmailVerificationToken }, Request.Scheme);

        TempData["Success"] = "Account created! Please log in.";
        return RedirectToAction(nameof(Login));
    }

    /* ---------------- Password reset (dev-mode links; wire SMTP for prod) ---------------- */

[HttpGet]
public IActionResult ForgotPassword() => View();

[EnableRateLimiting("auth")]
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _auth.FindByEmailAsync(email);
        if (user is not null)
        {
            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);
            await _auth.SaveUserAsync(user);
            // DEV-ONLY: show the reset link since there's no SMTP configured.
            // In production this would be emailed instead.
            TempData["DevResetLink"] = Url.Action("ResetPassword", "Home",
                new { token = user.PasswordResetToken }, Request.Scheme);
        }
        TempData["Success"] = "If that email exists, a reset link was sent.";
        return RedirectToAction(nameof(Login));
    }

[HttpGet]
public async Task<IActionResult> ResetPassword(string token)
    {
        var user = await _auth.FindByResetTokenAsync(token);
        if (user is null) return NotFound();
        return View(new ResetPasswordViewModel { Token = token });
    }

[EnableRateLimiting("auth")]
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(ResetPasswordViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var user = await _auth.FindByResetTokenAsync(m.Token);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired reset link.");
            return View(m);
        }
        user.PasswordHash = BLL.Services.PasswordHasher.Hash(m.Password);
        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;
        await _auth.SaveUserAsync(user);
        TempData["Success"] = "Password reset — please log in.";
        return RedirectToAction(nameof(Login));
    }

[HttpGet]
public async Task<IActionResult> VerifyEmail(string token)
    {
        var user = await _auth.FindByEmailVerificationTokenAsync(token);
        if (user is not null && user.EmailVerifiedAt is null)
        {
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.IsVerified = true;
            await _auth.SaveUserAsync(user);
            TempData["Success"] = "Email verified!";
        }
        return RedirectToAction(nameof(Login));
    }

[Authorize]
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

private string AttemptKey(string email) => $"login-attempts:{email.ToLowerInvariant()}";

private bool IsLockedOut(string email)
    {
        var key = AttemptKey(email);
        return _cache.TryGetValue(key, out int attempts) && attempts >= MaxAttempts;
    }

private void RecordFailure(string email)
    {
        var key = AttemptKey(email);
        var attempts = _cache.TryGetValue(key, out int a) ? a + 1 : 1;
        _cache.Set(key, attempts, LockoutDuration);
    }

private void ClearAttempts(string email) => _cache.Remove(AttemptKey(email));
}
