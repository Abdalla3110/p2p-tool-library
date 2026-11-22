using System.Security.Claims;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL.Models.ViewModels;
using BLL.Interfaces;

namespace PL.Controllers;

[Authorize]
public class MemberController : Controller
{
    private readonly IDisputeService _disputes;
    private readonly IReservationService _reservations;
    private readonly IPromoCodeService _promos;
    private readonly IMessageService _messages;
    private readonly INotificationService _notifications;
    private readonly IReviewService _reviews;
    private readonly IUserService _users;
    private readonly IToolService _tools;
    private readonly IToolDocumentService _toolDocs;
    private readonly ICategoryService _categories;
    private readonly IZoneService _zones;
    private readonly IDamageReportService _damageReports;
    private readonly IClaimService _claims;

    public MemberController(IDisputeService disputes,
        IReservationService reservations, IPromoCodeService promos,
        IMessageService messages, INotificationService notifications,
        IReviewService reviews, IUserService users,
        IToolService tools, IToolDocumentService toolDocs,
        ICategoryService categories, IZoneService zones,
        IDamageReportService damageReports, IClaimService claims)
    {
        _disputes = disputes;
        _reservations = reservations;
        _promos = promos;
        _messages = messages;
        _notifications = notifications;
        _reviews = reviews;
        _users = users;
        _tools = tools;
        _toolDocs = toolDocs;
        _categories = categories;
        _zones = zones;
        _damageReports = damageReports;
        _claims = claims;
    }

private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /* ---------------- Dashboard ---------------- */


private static readonly HashSet<string> AllowedImageExts = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };


private static void ApplyForm(Tool t, ToolFormViewModel m)
    {
        t.Name = m.Name.Trim();
        t.Brand = m.Brand;
        t.Description = m.Description;
        t.CategoryId = m.CategoryId;
        t.ZoneId = m.ZoneId;
        t.Condition = m.Condition;
        t.PricePerHour = m.PricePerHour;
        t.PricePerDay = m.PricePerDay;
        t.PricePerWeek = m.PricePerWeek;
        t.DepositAmount = m.DepositAmount;
        t.InsurancePercentage = m.InsurancePercentage;
        t.MaxBorrowDays = m.MaxBorrowDays;
        t.CleaningBufferHours = m.CleaningBufferHours;
        t.BatteryHealthPercentage = m.BatteryHealthPercentage;
        t.WarrantyExpiryDate = m.WarrantyExpiryDate;
        t.IsHighValue = m.IsHighValue;
    }

private async Task LoadToolFormData()
    {
        ViewBag.Categories = new SelectList(await _categories.GetAllAsync(), "CategoryId", "Name");
        ViewBag.Zones = new SelectList(await _zones.GetAllAsync(), "ZoneId", "Name");
        ViewBag.Conditions = new SelectList(new[] { "Excellent", "Good", "Fair" });
    }

    /* ---------------- My Bookings ---------------- */

public async Task<IActionResult> Index()
    {
        var uid = CurrentUserId;
        var dash = await _users.GetDashboardAsync(uid);
        if (dash.User is null) return Forbid();

        ViewBag.User = dash.User;
        ViewBag.MyToolsCount = dash.MyToolsCount;
        ViewBag.ActiveLoans = dash.ActiveLoans;
        ViewBag.ActiveLends = dash.ActiveLends;
        ViewBag.Upcoming = dash.Upcoming;
        ViewBag.Notifications = dash.Notifications;
        return View();
    }

    /* ---------------- My Tools ---------------- */

public async Task<IActionResult> Tools()
    {
        var tools = await _tools.GetMyToolsAsync(CurrentUserId);
        return View(tools);
    }

public async Task<IActionResult> ToolCreate()
    {
        await LoadToolFormData();
        return View("ToolForm", new ToolFormViewModel());
    }

public async Task<IActionResult> ToolEdit(int id)
    {
        var tool = await _tools.GetOwnedToolAsync(id, CurrentUserId);
        if (tool is null) return NotFound();
        await LoadToolFormData();
        return View("ToolForm", ToolFormViewModel.FromTool(tool));
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> ToolSave(ToolFormViewModel m, List<IFormFile>? Photos)
    {
        if (!ModelState.IsValid)
        {
            await LoadToolFormData();
            return View("ToolForm", m);
        }

        Tool tool;
        try
        {
            tool = await _tools.SaveToolAsync(m, CurrentUserId);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        // save uploaded photos
        if (Photos is { Count: > 0 })
        {
            var existing = await _toolDocs.GetForToolAsync(tool.ToolId);
            var sort = existing.Count;
            var hasPrimary = existing.Any(p => p.IsPrimary);
            foreach (var file in Photos.Where(f => f.Length > 0).Take(8))
            {
                await _toolDocs.SaveUploadedPhotoAsync(tool.ToolId, file, sort,
                    isPrimary: !hasPrimary && sort == 0);
                sort++;
            }
        }

        TempData["Success"] = "Tool saved.";
        return RedirectToAction(nameof(Tools));
    }

public async Task<IActionResult> Bookings()
    {
        var bookings = await _reservations.GetMyBookingsAsync(CurrentUserId);
        return View(bookings);
    }

    /* ---------------- Wallet ---------------- */

public async Task<IActionResult> Wallet()
    {
        var uid = CurrentUserId;
        ViewBag.User = await _users.GetProfileAsync(uid);
        var wallet = await _users.GetWalletAsync(uid);
        return View(wallet?.Transactions ?? new List<Transaction>());
    }

    /* ---------------- Profile ---------------- */

public async Task<IActionResult> Profile()
    {
        var user = await _users.GetProfileAsync(CurrentUserId);
        if (user is null) return Forbid();
        ViewBag.Zones = new SelectList(await _zones.GetAllAsync(), "ZoneId", "Name", user.ZoneId);
        return View(ProfileViewModel.FromUser(user));
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Profile(ProfileViewModel m, IFormFile? kycIdPhoto, IFormFile? kycSelfie, string? kycNationalId)
    {
        var uid = CurrentUserId;
        if (await _users.GetProfileAsync(uid) is null) return Forbid();
        if (!ModelState.IsValid)
        {
            ViewBag.Zones = new SelectList(await _zones.GetAllAsync(), "ZoneId", "Name", m.ZoneId);
            return View(m);
        }

        // KYC document uploads
        string? kycIdPath = null, kycSelfiePath = null;
        if (kycIdPhoto is { Length: > 0 })
            kycIdPath = await _users.SaveKycPhotoAsync(uid, kycIdPhoto, "id");
        if (kycSelfie is { Length: > 0 })
            kycSelfiePath = await _users.SaveKycPhotoAsync(uid, kycSelfie, "selfie");

        await _users.UpdateProfileAsync(uid, m, kycIdPath, kycSelfiePath, kycNationalId);
        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> ChangePassword(ChangePasswordViewModel m)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Passwords do not match or are too short.";
            return RedirectToAction(nameof(Profile));
        }
        var (ok, err) = await _users.ChangePasswordAsync(CurrentUserId, m.CurrentPassword, m.NewPassword);
        if (!ok)
        {
            TempData["Error"] = err == "WRONG_PASSWORD" ? "Current password is incorrect." : "Passwords do not match or are too short.";
            return RedirectToAction(nameof(Profile));
        }
        TempData["Success"] = "Password changed.";
        return RedirectToAction(nameof(Profile));
    }

    /* ---------------- Ratings ---------------- */

public async Task<IActionResult> Ratings()
    {
        var uid = CurrentUserId;
        ViewBag.Received = await _reviews.GetReceivedAsync(CurrentUserId);
        ViewBag.Given = await _reviews.GetGivenAsync(CurrentUserId);
        return View();
    }

    /* ---------------- Referrals ---------------- */

public async Task<IActionResult> Referral()
    {
        var uid = CurrentUserId;
        var me = await _users.GetProfileAsync(uid);
        ViewBag.Code = me?.ReferralCode;
        var refs = await _users.GetReferralListAsync(uid);
        ViewBag.TotalEarned = refs.Sum(r => r.CreditAmount);
        ViewBag.CompletedCount = refs.Count(r => r.Status == ReferralStatusEnum.Completed);
        ViewBag.Referrals = refs;
        return View();
    }

    /* ---------------- Wallet top-up (simulated gateway) ---------------- */

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> TopUp(decimal amount)
    {
        if (amount < 10 || amount > 50000)
        {
            TempData["Error"] = "Top-up amount must be between 10 and 50,000 EGP.";
            return RedirectToAction(nameof(Wallet));
        }
        var balance = await _users.TopUpAsync(CurrentUserId, amount);
        if (balance == 0) return Forbid();
        TempData["Success"] = $"Wallet topped up by {amount:0.00} EGP.";
        return RedirectToAction(nameof(Wallet));
    }

public async Task<IActionResult> UnreadCount()
    {
        var count = await _notifications.GetUnreadCountAsync(CurrentUserId);
        return Json(new { count });
    }

    /* ---------------- Notifications ---------------- */

public async Task<IActionResult> Notifications()
    {
        var items = await _notifications.GetForUserAsync(CurrentUserId, 100);
        return View(items);
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> MarkAllRead()
    {
        await _notifications.MarkAllReadAsync(CurrentUserId);
        return RedirectToAction(nameof(Notifications));
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> BookTool(int toolId, DateTime pickupDate, DateTime returnDate)
    {
        var back = Url.Action("Details", "Home", new { id = toolId })!;
        var result = await _reservations.CreateBookingAsync(toolId, CurrentUserId, pickupDate, returnDate,
            bid => Url.Action("Details", "Member", new { id = bid })!);

        if (result.Error is null) return RedirectToAction(nameof(Details), new { id = result.Booking!.BookingId });
        return result.Error switch
        {
            "NOT_FOUND" => NotFound(),
            "PAUSED" => RedirectWithError(back, "The platform is currently paused — new bookings are temporarily disabled."),
            "OWN_TOOL" => RedirectWithError(back, "You cannot book your own tool."),
            "BAD_DATES" => RedirectWithError(back, "Return date must be after pickup date."),
            _ when result.Error.StartsWith("MAX_DAYS:") => RedirectWithError(back, $"This tool can be borrowed for at most {result.Error["MAX_DAYS:".Length..]} days."),
            "CONFLICT" => RedirectWithError(back, "These dates are already booked. Please choose different dates."),
            "UNAVAILABLE" => RedirectWithError(back, "The tool is marked unavailable during these dates."),
            "RACE" => RedirectWithError(back, "These dates were just taken by another booking. Please choose different dates."),
            _ => RedirectWithError(back, "Could not create the booking."),
        };
    }

public async Task<IActionResult> Details(int id)
    {
        var booking = await _reservations.GetBookingWithPartiesAsync(id);
        if (booking is null) return NotFound();

        var uid = CurrentUserId;
        var isParty = uid == booking.BorrowerUserId || uid == booking.LenderUserId;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Librarian");
        if (!isParty && !isStaff) return Forbid();

        var ratedUserId = uid == booking.BorrowerUserId ? booking.LenderUserId : booking.BorrowerUserId;
        var hasRated = await _reviews.HasRatedBookingAsync(id, uid, ratedUserId);

        return View(new BookingDetailsViewModel
        {
            Booking = booking,
            Tool = booking.Tool,
            Borrower = booking.BorrowerUser,
            Lender = booking.LenderUser,
            CanCancel = isParty && booking.Status is BookingStatusEnum.Pending or BookingStatusEnum.Confirmed or BookingStatusEnum.WaitingHandover,
            HasRated = hasRated,
            Handovers = booking.Handovers.OrderBy(h => h.Type).ToList(),
        });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> RateTool(int id, int rating, string? comment)
    {
        var (ok, _) = await _reviews.RateToolAsync(id, CurrentUserId, rating, comment);
        if (ok) TempData["Success"] = "Thanks for rating the tool!";
        return RedirectToAction(nameof(Details), new { id });
    }

    /* ---------------- Checkout + promos ---------------- */

[HttpGet]
public async Task<IActionResult> Checkout(int id)
    {
        var booking = await _reservations.GetBookingForCheckoutAsync(id);
        if (booking is null) return NotFound();
        if (booking.BorrowerUserId != CurrentUserId) return Forbid();
        if (booking.Status is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed))
            return RedirectToAction(nameof(Details), new { id });

        ViewBag.Wallet = await _users.GetWalletBalanceAsync(CurrentUserId);
        ViewBag.AvailablePromos = await _promos.GetAvailableAsync();
        return View(booking);
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> ApplyPromo(int id, string code)
    {
        var result = await _promos.ApplyAsync(id, code, CurrentUserId);
        switch (result.Error)
        {
            case "NOT_FOUND": return NotFound();
            case "FORBIDDEN": return Forbid();
            case "INVALID": TempData["Error"] = "Invalid or expired promo code."; break;
            case "WRONG_CATEGORY": TempData["Error"] = "This promo doesn't apply to this tool's category."; break;
            case "BAD_STATUS": TempData["Error"] = "Promo can't be applied to this booking."; break;
            default: TempData["Success"] = $"Promo applied — you saved {result.Discount:0.00} EGP."; break;
        }
        return RedirectToAction(nameof(Checkout), new { id });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> RemovePromo(int id)
    {
        await _promos.RemoveAsync(id, CurrentUserId);
        return RedirectToAction(nameof(Checkout), new { id });
    }

    /// <summary>Borrower pays the booking: wallet debit + escrow deposit + confirm.</summary>

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Pay(int id)
    {
        var result = await _reservations.PayBookingAsync(id, CurrentUserId,
            bid => Url.Action("Details", "Member", new { id = bid })!);
        switch (result.Error)
        {
            case "NOT_FOUND": return NotFound();
            case "FORBIDDEN": return Forbid();
            case "INSUFFICIENT_FUNDS":
                TempData["Error"] = $"Insufficient wallet balance — you need {(result.Booking!.FinalAmountDue + result.Booking.DepositAmount):0.00} EGP (rental + deposit). Top up your wallet to complete payment.";
                break;
        }
        if (result.Success) TempData["Success"] = "Payment completed — booking confirmed.";
        return RedirectToAction(nameof(Details), new { id });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> ReportDamage(int id, string? description, IFormFile? photo)
    {
        var uid = CurrentUserId;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Librarian");
        var photoUrl = await SaveDamagePhoto(photo);
        var result = await _damageReports.ReportAsync(id, uid, isStaff, description, photoUrl);
        if (result.Error == "NOT_FOUND") return NotFound();
        if (result.Error == "FORBIDDEN") return Forbid();

        TempData["Success"] = "Damage reported — dispute opened and the librarian team will review it.";
        return RedirectToAction(nameof(Details), new { id });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Rate(int id, int rating, string? comment)
    {
        var (ok, err) = await _reviews.RateBookingAsync(id, CurrentUserId, rating, comment);
        if (ok) TempData["Success"] = "Thanks for your rating!";
        else if (err == "NOT_COMPLETED") TempData["Error"] = "You can only rate after the booking is completed.";
        return RedirectToAction(nameof(Details), new { id });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Cancel(int id)
    {
        var result = await _reservations.CancelBookingAsync(id, CurrentUserId);
        switch (result.Error)
        {
            case "NOT_FOUND": return NotFound();
            case "FORBIDDEN": return Forbid();
            case "CANNOT_CANCEL":
                TempData["Error"] = "This booking can't be cancelled — use a dispute or wait for return instead.";
                break;
            default:
                if (result.RefundAmount > 0)
                    TempData["Success"] = $"Booking cancelled. Refund of {result.RefundAmount:0.00} EGP issued to your wallet (fee {result.FeeAmount:0.00} EGP).";
                else
                    TempData["Success"] = "Booking cancelled.";
                break;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Reschedule(int id, DateTime pickupDate, DateTime returnDate)
    {
        var result = await _reservations.RescheduleBookingAsync(id, CurrentUserId, pickupDate, returnDate);
        switch (result.Error)
        {
            case "NOT_FOUND": return NotFound();
            case "FORBIDDEN": return Forbid();
            case "CANNOT_RESCHEDULE": TempData["Error"] = "This booking can't be rescheduled."; break;
            case "BAD_DATES": TempData["Error"] = "Invalid date range."; break;
            case "CONFLICT": TempData["Error"] = "The tool isn't available for those dates."; break;
            case "RACE": TempData["Error"] = "The tool isn't available for those dates."; break;
            default: TempData["Success"] = "Booking rescheduled."; break;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

private IActionResult RedirectWithError(string url, string message)
    {
        TempData["Error"] = message;
        return Redirect(url);
    }

    private async Task<string?> SaveDamagePhoto(IFormFile? photo)
    {
        if (photo is not { Length: > 0 }) return null;
        var ext = Path.GetExtension(photo.FileName);
        if (!AllowedImageExts.Contains(ext)) return null;
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "damage");
        Directory.CreateDirectory(dir);
        var fname = $"{Guid.NewGuid():N}{ext}";
        await using (var fs = System.IO.File.Create(Path.Combine(dir, fname)))
            await photo.CopyToAsync(fs);
        return $"/uploads/damage/{fname}";
    }

public async Task<IActionResult> MyDisputes()
    {
        var mine = await _disputes.GetMyDisputesAsync(CurrentUserId);
        return View(mine);
    }

public async Task<IActionResult> OpenDispute(int bookingId)
    {
        var booking = await _reservations.GetBookingForDisputeAsync(bookingId);
        if (booking is null) return NotFound();
        var uid = CurrentUserId;
        if (uid != booking.BorrowerUserId && uid != booking.LenderUserId) return Forbid();

        ViewBag.Booking = booking;
        ViewBag.Types = new[] { "Damage", "Payment", "LateReturn", "Other" };
        return View(new DisputeFormViewModel { BookingId = bookingId });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> OpenDispute(DisputeFormViewModel m)
    {
        var booking = await _reservations.GetBookingForDisputeAsync(m.BookingId);
        if (booking is null) return NotFound();
        var uid = CurrentUserId;
        if (uid != booking.BorrowerUserId && uid != booking.LenderUserId) return Forbid();
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(m.Description))
        {
            ViewBag.Booking = booking;
            ViewBag.Types = new[] { "Damage", "Payment", "LateReturn", "Other" };
            return View(m);
        }

        var dispute = await _disputes.OpenDisputeAsync(
            m.BookingId, uid, m.Type, m.Description.Trim());

        TempData["Success"] = $"Dispute #{dispute.DisputeId} opened — a librarian will review it.";
        return RedirectToAction(nameof(MyDisputes));
    }

public async Task<IActionResult> MyClaims()
    {
        var claims = await _claims.GetMyClaimsAsync(CurrentUserId);
        return View(claims);
    }

public async Task<IActionResult> FileClaim(int bookingId)
    {
        var booking = await _reservations.GetBookingForClaimAsync(bookingId, includeZone: false);
        if (booking is null) return NotFound();
        if (booking.BorrowerUserId != CurrentUserId && booking.LenderUserId != CurrentUserId) return Forbid();

        ViewBag.Booking = booking;
        return View(new InsuranceClaimFormViewModel { BookingId = bookingId, ToolId = booking.ToolId });
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> FileClaim(InsuranceClaimFormViewModel m)
    {
        var booking = await _reservations.GetBookingForClaimAsync(m.BookingId, includeZone: true);
        if (booking is null) return NotFound();
        var uid = CurrentUserId;
        if (uid != booking.BorrowerUserId && uid != booking.LenderUserId) return Forbid();
        if (!ModelState.IsValid)
        {
            ViewBag.Booking = booking;
            return View(m);
        }

        var result = await _claims.SubmitAsync(m, uid);
        if (result.Error == "NOT_FOUND") return NotFound();
        if (result.Error == "FORBIDDEN") return Forbid();

        TempData["Success"] = "Claim submitted — the librarian team will review it.";
        return RedirectToAction(nameof(MyClaims));
    }

public async Task<IActionResult> Messages()
    {
        var convs = await _messages.GetConversationsAsync(CurrentUserId);
        ViewBag.Conversations = convs.Select(c => new
        {
            c.ConversationId,
            Other = c.User1Id == CurrentUserId ? c.User2 : c.User1,
            c.Tool,
            c.Subject,
            LastMessage = c.Messages.FirstOrDefault()?.Body,
            c.LastMessageAt,
            Unread = c.Messages.Count(m => !m.IsRead && m.SenderUserId != CurrentUserId),
        }).ToList();
        return View();
    }

[HttpGet]
public async Task<IActionResult> StartConversation(int toolId)
    {
        var conv = await _messages.StartConversationAsync(toolId, CurrentUserId);
        if (conv is null)
        {
            if (!await _messages.ToolExistsAsync(toolId)) return NotFound();
            return RedirectToAction(nameof(Messages));
        }
        return RedirectToAction(nameof(Conversation), new { id = conv.ConversationId });
    }

public async Task<IActionResult> Conversation(Guid id)
    {
        var (conv, msgs) = await _messages.GetConversationAsync(id, CurrentUserId);
        if (conv is null)
        {
            return await _messages.ConversationExistsAsync(id) ? Forbid() : NotFound();
        }
        await _messages.MarkReadAsync(id, CurrentUserId);
        ViewBag.Conversation = conv;
        ViewBag.Other = conv.User1Id == CurrentUserId ? conv.User2 : conv.User1;
        return View(msgs);
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Send(Guid id, string body)
    {
        if (!await _messages.IsParticipantAsync(id, CurrentUserId))
            return await _messages.ConversationExistsAsync(id) ? Forbid() : NotFound();
        if (string.IsNullOrWhiteSpace(body)) return RedirectToAction(nameof(Conversation), new { id });
        await _messages.SendMessageAsync(id, CurrentUserId, User.Identity?.Name ?? "Member", body);
        return RedirectToAction(nameof(Conversation), new { id });
    }
}
