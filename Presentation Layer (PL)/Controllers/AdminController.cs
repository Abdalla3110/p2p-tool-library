using System.Security.Claims;
using BLL.Interfaces;
using DAL.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL.Models.ViewModels;

namespace PL.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _admin;
    private readonly ICategoryService _categories;
    private readonly IZoneService _zones;

    public AdminController(IAdminService admin, ICategoryService categories, IZoneService zones)
    {
        _admin = admin;
        _categories = categories;
        _zones = zones;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();

    /* ---------------- Dashboard ---------------- */

    public async Task<IActionResult> Index()
    {
        var d = await _admin.GetDashboardAsync();
        ViewBag.TotalUsers = d.TotalUsers;
        ViewBag.ActiveUsers = d.ActiveUsers;
        ViewBag.TotalTools = d.TotalTools;
        ViewBag.AvailableTools = d.AvailableTools;
        ViewBag.TotalBookings = d.TotalBookings;
        ViewBag.Gmv = d.Gmv;
        ViewBag.PlatformRevenue = d.PlatformRevenue;
        ViewBag.OpenDisputes = d.OpenDisputes;
        ViewBag.PendingTools = d.PendingTools;
        ViewBag.PendingKyc = d.PendingKyc;
        ViewBag.Monthly = d.Monthly;
        ViewBag.RecentActivity = d.RecentActivity;
        ViewBag.RecentTransactions = d.RecentTransactions;
        return View();
    }

    /* ---------------- Users ---------------- */

    public async Task<IActionResult> Users(string? role, string? search)
    {
        var page = await _admin.GetUsersPageAsync(role, search);
        ViewBag.Role = role;
        ViewBag.Search = search;
        ViewBag.Roles = page.Roles;
        ViewBag.Users = page.Users;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int id, int roleId)
    {
        await _admin.ChangeRoleAsync(id, roleId, CurrentUserId, Ip);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserActive(int id)
    {
        await _admin.ToggleUserActiveAsync(id, CurrentUserId, Ip);
        return RedirectToAction(nameof(Users));
    }

    /* ---------------- Roles ---------------- */

    public async Task<IActionResult> Roles()
    {
        var (roles, counts) = await _admin.GetRolesPageAsync();
        ViewBag.Counts = counts;
        ViewBag.Roles = roles;
        return View();
    }

    /* ---------------- Add user ---------------- */

    private async Task LoadAddUserForm()
    {
        ViewBag.Roles = new SelectList((await _admin.GetRolesPageAsync()).Roles, "RoleId", "Name");
        ViewBag.Zones = new SelectList(await _zones.GetAllAsync(), "ZoneId", "Name");
    }

    [HttpGet]
    public async Task<IActionResult> AddUser()
    {
        await LoadAddUserForm();
        return View(new AdminUserFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(AdminUserFormViewModel m)
    {
        if (!ModelState.IsValid || await _admin.EmailExistsAsync(m.Email))
        {
            if (ModelState.IsValid)
                ModelState.AddModelError("Email", "Email already registered.");
            await LoadAddUserForm();
            return View(m);
        }

        var user = await _admin.CreateUserAsync(m);
        TempData["Success"] = $"User {user.FullName} created.";
        return RedirectToAction(nameof(Users));
    }

    /* ---------------- Settings ---------------- */

    public async Task<IActionResult> Settings()
    {
        return View(await _admin.GetSettingsAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(PlatformSetting m)
    {
        await _admin.SaveSettingsAsync(m, CurrentUserId);
        TempData["Success"] = "Settings saved.";
        return RedirectToAction(nameof(Settings));
    }

    /* ---------------- Audit log ---------------- */

    public async Task<IActionResult> AuditLogs(string? action, int? userId)
    {
        var page = await _admin.GetAuditLogPageAsync(action, userId);
        ViewBag.Action = action;
        ViewBag.UserId = userId;
        ViewBag.Users = page.Users;
        ViewBag.Logs = page.Logs;
        return View();
    }

    /* ---------------- Revenue ---------------- */

    public async Task<IActionResult> Revenue()
    {
        var page = await _admin.GetRevenuePageAsync();
        ViewBag.Reports = page.Reports;
        ViewBag.Monthly = page.Monthly;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateRevenueReport()
    {
        await _admin.GenerateRevenueReportAsync(CurrentUserId);
        TempData["Success"] = "Revenue report generated.";
        return RedirectToAction(nameof(Revenue));
    }

    /* ---------------- Promotions ---------------- */

    public async Task<IActionResult> Promotions()
    {
        var promos = await _admin.GetPromotionsAsync();
        return View(promos);
    }

    [HttpGet]
    public async Task<IActionResult> PromotionForm(int? id)
    {
        PromotionFormViewModel m;
        if (id.HasValue)
        {
            var p = await _admin.GetPromotionForEditAsync(id.Value);
            if (p is null) return NotFound();
            m = new PromotionFormViewModel
            {
                PromotionId = p.PromotionId, Code = p.Code, Description = p.Description,
                DiscountType = p.DiscountType, DiscountPercentage = p.DiscountPercentage,
                DiscountAmount = p.DiscountAmount, CategoryId = p.CategoryId,
                StartDate = p.StartDate, EndDate = p.EndDate, MaxUses = p.MaxUses,
                Status = p.Status,
            };
        }
        else m = new PromotionFormViewModel { StartDate = DateOnly.FromDateTime(DateTime.Today) };

        await LoadPromotionForm(m);
        return View(m);
    }

    private async Task LoadPromotionForm(PromotionFormViewModel m)
    {
        ViewBag.Categories = new SelectList(await _categories.GetAllAsync(), "CategoryId", "Name", m.CategoryId);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PromotionSave(PromotionFormViewModel m)
    {
        if (!ModelState.IsValid)
        {
            await LoadPromotionForm(m);
            return View("PromotionForm", m);
        }
        var result = await _admin.SavePromotionAsync(m, CurrentUserId);
        if (result.Error == "NOT_FOUND") return NotFound();
        if (result.Error == "CODE_EXISTS")
        {
            ModelState.AddModelError("Code", "Code already exists.");
            await LoadPromotionForm(m);
            return View("PromotionForm", m);
        }
        TempData["Success"] = "Promotion saved.";
        return RedirectToAction(nameof(Promotions));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePromotion(int id)
    {
        await _admin.TogglePromotionAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Promotions));
    }

    /* ---------------- Bookings / Transactions / Tools ---------------- */

    public async Task<IActionResult> Bookings(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Statuses = new[] { "Pending", "Confirmed", "WaitingHandover", "Active", "Completed", "Cancelled", "Disputed" };
        ViewBag.Bookings = await _admin.GetAllBookingsAsync(status);
        return View();
    }

    public async Task<IActionResult> Transactions(string? type)
    {
        ViewBag.Type = type;
        ViewBag.Types = new[] { "Payment", "Credit", "Deposit", "Refund", "Fine", "Commission" };
        ViewBag.Transactions = await _admin.GetTransactionsAsync(type);
        return View();
    }

    public async Task<IActionResult> Tools()
    {
        ViewBag.Tools = await _admin.GetAllToolsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendTool(int id)
    {
        await _admin.SuspendToolAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Tools));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReinstateTool(int id)
    {
        await _admin.ReinstateToolAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Tools));
    }

    /* ---------------- Activity ---------------- */

    public async Task<IActionResult> Activity()
    {
        var feed = await _admin.GetActivityFeedAsync();
        ViewBag.Transactions = feed.Transactions;
        ViewBag.AuditLogs = feed.AuditLogs;
        ViewBag.Notifications = feed.Notifications;
        return View();
    }
}
