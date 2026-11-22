using System.Security.Claims;
using BLL.Interfaces;
using DAL.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL.Models.ViewModels;

namespace PL.Controllers;

[Authorize(Roles = "Technician,Librarian,Admin")]
public class MaintenanceController : Controller
{
    private readonly IMaintenanceService _maintenance;

    public MaintenanceController(IMaintenanceService maintenance) => _maintenance = maintenance;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /* ---------------- Dashboard ---------------- */

    public async Task<IActionResult> Index()
    {
        var d = await _maintenance.GetDashboardAsync(CurrentUserId);
        ViewBag.ToolsInMaintenance = d.ToolsInMaintenance;
        ViewBag.UnfitTools = d.UnfitTools;
        ViewBag.OpenRecords = d.OpenRecords;
        ViewBag.MyCompleted = d.MyCompleted;
        ViewBag.OpenRepairs = d.OpenRepairs;
        ViewBag.AvgHours = d.AvgHours;
        ViewBag.MyRecords = d.MyRecords;
        ViewBag.NeedsAttention = d.NeedsAttention;
        return View();
    }

    /* ---------------- Tools health ---------------- */

    public async Task<IActionResult> Tools(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Statuses = new[] { "Available", "Rented", "Maintenance", "Locked", "Unfit", "PendingApproval" };
        ViewBag.Tools = await _maintenance.GetToolsForHealthAsync(status);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status)
    {
        await _maintenance.SetToolStatusAsync(id, status);
        return RedirectToAction(nameof(Tools));
    }

    /* ---------------- Maintenance records ---------------- */

    public async Task<IActionResult> Records(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Statuses = new[] { "Scheduled", "InProgress", "Completed", "NeedsAttention", "Failed", "RetestNeeded" };
        ViewBag.Records = await _maintenance.GetRecordsAsync(status);
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> RecordForm(int? toolId)
    {
        await LoadRecordFormData(toolId);
        return View(new MaintenanceRecordFormViewModel { ToolId = toolId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordSave(MaintenanceRecordFormViewModel m)
    {
        if (!ModelState.IsValid)
        {
            await LoadRecordFormData(m.ToolId);
            return View("RecordForm", m);
        }

        await _maintenance.SaveRecordAsync(m, CurrentUserId);
        TempData["Success"] = "Maintenance record saved.";
        return RedirectToAction(nameof(Records));
    }

    private async Task LoadRecordFormData(int? toolId)
    {
        var tools = (await _maintenance.GetActiveToolsAsync())
            .Select(t => new { t.ToolId, t.Name }).ToList();
        ViewBag.Tools = new SelectList(tools, "ToolId", "Name", toolId);
        ViewBag.Types = new SelectList(new[] { "Inspection", "Repair", "Cleaning", "Battery", "External" });
        ViewBag.Statuses = new SelectList(new[] { "Scheduled", "InProgress", "Completed", "NeedsAttention", "Failed", "RetestNeeded" });
    }

    /* ---------------- External repairs ---------------- */

    public async Task<IActionResult> Repairs(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Statuses = new[] { "Inspecting", "WaitingParts", "Completed", "Cancelled" };
        ViewBag.Repairs = await _maintenance.GetRepairsAsync(status);
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> RepairForm(int? toolId)
    {
        var tools = (await _maintenance.GetActiveToolsAsync())
            .Select(t => new { t.ToolId, t.Name }).ToList();
        ViewBag.Tools = new SelectList(tools, "ToolId", "Name", toolId);
        return View(new ExternalRepairFormViewModel { ToolId = toolId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RepairSave(ExternalRepairFormViewModel m, IFormFile? invoice)
    {
        if (!ModelState.IsValid)
        {
            var tools = (await _maintenance.GetActiveToolsAsync())
                .Select(t => new { t.ToolId, t.Name }).ToList();
            ViewBag.Tools = new SelectList(tools, "ToolId", "Name", m.ToolId);
            return View("RepairForm", m);
        }

        var invoiceUrl = invoice is { Length: > 0 } ? await _maintenance.SaveInvoiceAsync(invoice) : null;
        await _maintenance.SaveRepairAsync(m, invoiceUrl);
        TempData["Success"] = "Repair record saved.";
        return RedirectToAction(nameof(Repairs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnRepair(int id, DateOnly? actualReturnAt, decimal? cost)
    {
        await _maintenance.ReturnRepairAsync(id, actualReturnAt, cost);
        return RedirectToAction(nameof(Repairs));
    }

    /* ---------------- Disposals ---------------- */

    public async Task<IActionResult> Disposals()
    {
        ViewBag.Disposals = await _maintenance.GetDisposalsAsync();
        ViewBag.Tools = (await _maintenance.GetActiveToolsAsync())
            .Select(t => new { t.ToolId, t.Name, t.Status }).ToList();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDisposal(int toolId, DisposalReasonEnum reason, string? reasonDescription, string? recycleCenter, bool cancelFutureBookings)
    {
        await _maintenance.CreateDisposalAsync(toolId, reason, reasonDescription, recycleCenter, cancelFutureBookings);
        TempData["Success"] = "Disposal request submitted for approval.";
        return RedirectToAction(nameof(Disposals));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveDisposal(int id)
    {
        await _maintenance.ApproveDisposalAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Disposals));
    }

    /* ---------------- Technician profile ---------------- */

    public async Task<IActionResult> Profile()
    {
        var v = await _maintenance.GetTechnicianProfileAsync(CurrentUserId);
        ViewBag.User = v.User;
        ViewBag.Profile = v.Profile;
        ViewBag.TotalRepairs = v.TotalRepairs;
        ViewBag.AvgHours = v.AvgHours;
        ViewBag.OpenRepairs = v.OpenRepairs;
        ViewBag.TotalToolsMaintained = v.TotalToolsMaintained;
        return View();
    }
}
