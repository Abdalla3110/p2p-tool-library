using System.Security.Claims;
using BLL.Interfaces;
using DAL.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers;

[Authorize(Roles = "Librarian,Admin")]
public class LibraryController : Controller
{
    private readonly ILibrarianService _librarian;
    private readonly IUserService _users;
    private readonly IToolService _tools;
    private readonly IReservationService _reservations;
    private readonly IZoneService _zones;

    public LibraryController(ILibrarianService librarian, IUserService users, IToolService tools,
        IReservationService reservations, IZoneService zones)
    {
        _librarian = librarian;
        _users = users;
        _tools = tools;
        _reservations = reservations;
        _zones = zones;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    /* ---------------- Dashboard ---------------- */

    public async Task<IActionResult> Index()
    {
        var d = await _librarian.GetDashboardAsync(CurrentUserId);
        ViewBag.OpenDisputes = d.OpenDisputes;
        ViewBag.PendingAudits = d.PendingAudits;
        ViewBag.PendingTools = d.PendingTools;
        ViewBag.PendingKyc = d.PendingKyc;
        ViewBag.TodayHandovers = d.TodayHandovers;
        ViewBag.OpenLateReturns = d.OpenLateReturns;
        ViewBag.MyTasks = d.MyTasks;
        ViewBag.RecentDisputes = d.RecentDisputes;
        return View();
    }

    /* ---------------- Tasks ---------------- */

    public async Task<IActionResult> Tasks()
    {
        var tasks = await _librarian.GetMyTasksAsync(CurrentUserId);
        return View(tasks);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTask(int id)
    {
        await _librarian.CompleteTaskAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Tasks));
    }

    /* ---------------- Disputes ---------------- */

    public async Task<IActionResult> Disputes(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Disputes = await _librarian.GetDisputesAsync(status);
        ViewBag.Statuses = new[] { "Open", "InReview", "Resolved", "Rejected", "Reopened" };
        return View();
    }

    public async Task<IActionResult> DisputeDetails(int id)
    {
        var d = await _librarian.GetDisputeForReviewAsync(id, CurrentUserId);
        if (d is null) return NotFound();
        return View(d);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveDispute(int id, string resolution, string? notes, decimal? refundAmount)
    {
        if (!Enum.TryParse<DisputeResolutionEnum>(resolution, out var parsed))
        {
            TempData["Error"] = "Invalid resolution.";
            return RedirectToAction(nameof(Disputes));
        }
        await _librarian.ResolveDisputeAsync(id, parsed, notes, refundAmount);
        TempData["Success"] = "Dispute resolved.";
        return RedirectToAction(nameof(Disputes));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmBooking(int id)
    {
        var ok = await _librarian.ConfirmBookingAsync(id);
        if (ok) TempData["Success"] = "Booking confirmed.";
        return RedirectToAction(nameof(Bookings));
    }

    /* ---------------- Inventory Audits ---------------- */

    public async Task<IActionResult> Audits(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Audits = await _librarian.GetAuditsAsync(status);
        ViewBag.Statuses = new[] { "Pending", "PhotoSubmitted", "Approved", "Rejected", "Failed" };
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewAudit(int id, string decision, string? notes)
    {
        if (!await _librarian.ReviewAuditAsync(id, decision, notes, CurrentUserId)) return NotFound();
        TempData["Success"] = "Audit reviewed.";
        return RedirectToAction(nameof(Audits));
    }

    /* ---------------- Handovers ---------------- */

    public async Task<IActionResult> Handovers()
    {
        ViewBag.Handovers = await _librarian.GetHandoversAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyHandover(int id, string qrCode,
        bool matchesListing, bool accessories, bool noDamage, bool bothPresent,
        IFormFile? pickupPhoto, IFormFile? returnPhoto)
    {
        var staffId = CurrentUserId;
        var (ok, err) = await _librarian.VerifyHandoverAsync(id, qrCode,
            matchesListing, accessories, noDamage, bothPresent, pickupPhoto, returnPhoto);
        if (ok)
        {
            await _librarian.StampHandoverVerifierAsync(id, staffId);
            TempData["Success"] = "Handover verified.";
        }
        else
        {
            TempData["Error"] = err switch
            {
                "QR_MISMATCH" => "QR code does not match this handover.",
                "PHOTO_REQUIRED" => "A photo for this handover type is required to verify.",
                "BOTH_PRESENT_REQUIRED" => "Both parties must be present to verify a handover.",
                "CHECKLIST_INCOMPLETE" => "Complete the full checklist — report issues via damage report instead of verifying.",
                _ => "Handover not found.",
            };
        }
        return RedirectToAction(nameof(Handovers));
    }

    /* ---------------- Insurance Claims ---------------- */

    public async Task<IActionResult> Claims(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Claims = await _librarian.GetClaimsAsync(status);
        ViewBag.Statuses = new[] { "Open", "InReview", "Approved", "Rejected" };
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewClaim(int id, bool approve, string? notes, decimal? payout)
    {
        await _librarian.ReviewClaimAsync(id, approve, notes, payout);
        TempData["Success"] = "Claim reviewed.";
        return RedirectToAction(nameof(Claims));
    }

    /* ---------------- Late Returns ---------------- */

    public async Task<IActionResult> LateReturns(string? status)
    {
        ViewBag.Status = status;
        ViewBag.LateReturns = await _librarian.GetLateReturnsAsync(status);
        ViewBag.Statuses = new[] { "Warning", "FineApplied", "Frozen", "AccountFrozen", "Resolved" };
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveLateReturn(int id)
    {
        await _librarian.ResolveLateReturnAsync(id);
        return RedirectToAction(nameof(LateReturns));
    }

    /* ---------------- Blacklist ---------------- */

    public async Task<IActionResult> Blacklist()
    {
        ViewBag.Suspensions = await _users.GetSuspensionsAsync();
        ViewBag.Users = await _users.GetActiveUsersAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendUser(int userId, SuspensionReasonEnum reason, string? description, int? days)
    {
        var err = await _users.SuspendUserAsync(userId, reason, description, days, CurrentUserId);
        if (err == "SELF") TempData["Error"] = "You cannot suspend yourself.";
        else TempData["Success"] = "User suspended.";
        return RedirectToAction(nameof(Blacklist));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LiftSuspension(int id)
    {
        await _users.LiftSuspensionAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Blacklist));
    }

    /* ---------------- Tools approval ---------------- */

    public async Task<IActionResult> Tools(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Tools = await _tools.GetToolsForApprovalAsync(status);
        ViewBag.Statuses = new[] { "PendingApproval", "Available", "Rented", "Maintenance", "Locked", "Unfit" };
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTool(int id)
    {
        await _tools.ApproveToolAsync(id);
        return RedirectToAction(nameof(Tools));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectTool(int id)
    {
        await _tools.RejectToolAsync(id);
        return RedirectToAction(nameof(Tools));
    }

    /* ---------------- Users / KYC ---------------- */

    public async Task<IActionResult> Users(string? kyc)
    {
        ViewBag.Kyc = kyc;
        ViewBag.Users = await _users.GetUsersForKycAsync(kyc);
        ViewBag.KycStatuses = new[] { "Pending", "Verified", "Rejected" };
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewKyc(int id, bool approve)
    {
        await _users.ReviewKycAsync(id, approve, CurrentUserId);
        return RedirectToAction(nameof(Users));
    }

    /* ---------------- Bookings (all) ---------------- */

    public async Task<IActionResult> Bookings(string? status)
    {
        ViewBag.Status = status;
        ViewBag.Bookings = await _reservations.GetAllBookingsAsync(status);
        ViewBag.Statuses = new[] { "Pending", "Confirmed", "WaitingHandover", "Active", "Completed", "Cancelled", "Disputed" };
        return View();
    }

    /* ---------------- Broadcasts ---------------- */

    public async Task<IActionResult> Broadcasts()
    {
        ViewBag.Broadcasts = await _librarian.GetBroadcastsAsync();
        ViewBag.Zones = await _zones.GetAllAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendBroadcast(int? zoneId, string title, string message, string priority)
    {
        var recipients = await _librarian.SendBroadcastAsync(zoneId, title, message, priority, CurrentUserId);
        TempData["Success"] = $"Broadcast sent to {recipients} users.";
        return RedirectToAction(nameof(Broadcasts));
    }

    /* ---------------- Zones ---------------- */

    public async Task<IActionResult> Zones()
    {
        var overview = await _zones.GetOverviewAsync();
        ViewBag.Zones = overview.Select(r => r.Zone).ToList();
        ViewBag.MemberCounts = overview.ToDictionary(r => r.Zone.ZoneId, r => r.MemberCount);
        ViewBag.ToolCounts = overview.ToDictionary(r => r.Zone.ZoneId, r => r.ToolCount);
        return View();
    }
}
