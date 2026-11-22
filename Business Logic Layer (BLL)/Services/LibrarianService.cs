using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class LibrarianService : ILibrarianService
{
    private static readonly HashSet<string> HandoverPhotoExts = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly AppDbContext _db;
    public LibrarianService(AppDbContext db) => _db = db;

    public async Task<bool> ConfirmBookingAsync(int bookingId)
    {
        var b = await _db.Bookings.FindAsync(bookingId);
        if (b is null || b.Status != BookingStatusEnum.WaitingHandover) return false;
        b.Status = BookingStatusEnum.Confirmed;
        b.UpdatedAt = DateTime.UtcNow;
        _db.Notifications.Add(new Notification
        {
            UserId = b.BorrowerUserId, Type = "Booking", Title = "Booking approved",
            Message = $"Your booking #{bookingId} was approved by the library — handover is scheduled.",
            Priority = NotificationPriorityEnum.Normal, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Ok, string? Error)> VerifyHandoverAsync(int handoverId, string qrCode,
        bool matchesListing, bool accessories, bool noDamage, bool bothPresent,
        IFormFile? pickupPhoto, IFormFile? returnPhoto)
    {
        var h = await _db.Handovers.Include(x => x.Booking).Include(x => x.Tool)
            .FirstOrDefaultAsync(x => x.HandoverId == handoverId);
        if (h is null) return (false, "NOT_FOUND");

        if (!string.Equals(h.QrCode, qrCode?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            h.Status = HandoverStatusEnum.FailedScan;
            await _db.SaveChangesAsync();
            return (false, "QR_MISMATCH");
        }

        var requiredPhoto = h.Type == HandoverTypeEnum.Pickup ? pickupPhoto : returnPhoto;
        if (requiredPhoto is null || requiredPhoto.Length == 0)
            return (false, "PHOTO_REQUIRED");
        if (!bothPresent) return (false, "BOTH_PRESENT_REQUIRED");
        if (!matchesListing || !accessories || !noDamage)
            return (false, "CHECKLIST_INCOMPLETE");

        var now = DateTime.UtcNow;
        h.Status = HandoverStatusEnum.Verified;
        h.VerifiedAt = now;
        h.ChecklistMatchesListing = matchesListing;
        h.ChecklistAccessories = accessories;
        h.ChecklistNoDamage = noDamage;
        h.ChecklistBothPresent = bothPresent;
        var pickupUrl = await SaveHandoverPhoto(pickupPhoto, "pickup");
        var returnUrl = await SaveHandoverPhoto(returnPhoto, "return");
        if (pickupUrl is not null) h.PickupPhotoUrl = pickupUrl;
        if (returnUrl is not null) h.ReturnPhotoUrl = returnUrl;

        if (h.Booking is not null)
        {
            if (h.Type == HandoverTypeEnum.Pickup && h.Booking.Status == BookingStatusEnum.Confirmed)
            {
                h.Booking.Status = BookingStatusEnum.Active;
                if (h.Tool is not null) h.Tool.Status = ToolStatusEnum.Rented;
            }
            else if (h.Type == HandoverTypeEnum.Return && h.Booking.Status == BookingStatusEnum.Active)
            {
                h.Booking.Status = BookingStatusEnum.Completed;
                h.Booking.UpdatedAt = now;
                if (h.Tool is not null) h.Tool.Status = ToolStatusEnum.Available;

                if (h.Booking.DepositStatus == DepositStatusEnum.Held)
                {
                    h.Booking.DepositStatus = DepositStatusEnum.Released;
                    var lender = await _db.Users.FindAsync(h.Booking.LenderUserId);
                    if (lender is not null)
                    {
                        lender.WalletBalance += h.Booking.DepositAmount;
                        _db.Transactions.Add(new Transaction
                        {
                            UserId = lender.UserId, BookingId = h.Booking.BookingId,
                            Type = TransactionTypeEnum.Deposit, Amount = h.Booking.DepositAmount,
                            Status = TransactionStatusEnum.Released,
                            Description = $"Deposit released — booking #{h.Booking.BookingId}",
                            CreatedAt = now,
                        });
                    }
                }

                var pendingRefs = await _db.Referrals
                    .Where(r => r.ReferredUserId == h.Booking.BorrowerUserId && r.Status == ReferralStatusEnum.Pending)
                    .ToListAsync();
                foreach (var r in pendingRefs)
                {
                    r.Status = ReferralStatusEnum.Completed;
                    r.CompletedAt = now;
                    var referrer = await _db.Users.FindAsync(r.ReferrerUserId);
                    if (referrer is not null && r.CreditAmount > 0)
                    {
                        referrer.WalletBalance += r.CreditAmount;
                        _db.Transactions.Add(new Transaction
                        {
                            UserId = referrer.UserId, BookingId = h.Booking.BookingId,
                            Type = TransactionTypeEnum.Credit, Amount = r.CreditAmount,
                            BalanceAfter = referrer.WalletBalance, Status = TransactionStatusEnum.Completed,
                            Description = $"Referral bonus — user {r.ReferredUserId} completed booking #{h.Booking.BookingId}",
                            Reference = $"REF-{r.ReferralId}", CreatedAt = now,
                        });
                        _db.Notifications.Add(new Notification
                        {
                            UserId = referrer.UserId, Type = "Referral", Title = "Referral bonus earned",
                            Message = $"A user you referred completed their first booking — {r.CreditAmount:0} EGP credited.",
                            Priority = NotificationPriorityEnum.Normal, CreatedAt = now,
                        });
                    }
                }
            }
            h.Booking.UpdatedAt = now;
        }
        await _db.SaveChangesAsync();
        return (true, null);
    }

    /* ---------------- dashboard + tasks ---------------- */

    public async Task<LibraryDashboard> GetDashboardAsync(int librarianUserId)
    {
        var openStatuses = new[] { DisputeStatusEnum.Open, DisputeStatusEnum.InReview, DisputeStatusEnum.Reopened };
        var dash = new LibraryDashboard
        {
            OpenDisputes = await _db.Disputes.CountAsync(d => openStatuses.Contains(d.Status)),
            PendingAudits = await _db.InventoryAudits.CountAsync(a =>
                a.Status == AuditStatusEnum.Pending || a.Status == AuditStatusEnum.PhotoSubmitted),
            PendingTools = await _db.Tools.CountAsync(t => t.Status == ToolStatusEnum.PendingApproval),
            PendingKyc = await _db.Users.CountAsync(u => u.KycStatus == KycStatusEnum.Pending),
            TodayHandovers = await _db.Handovers.CountAsync(h =>
                h.ScheduledAt >= DateTime.Today && h.ScheduledAt < DateTime.Today.AddDays(1)
                && h.Status != HandoverStatusEnum.Verified),
            OpenLateReturns = await _db.LateReturns.CountAsync(l => l.Status != LateReturnStatusEnum.Resolved),
            MyTasks = await _db.LibrarianTasks
                .Where(t => t.LibrarianUserId == librarianUserId && t.Status != "Completed")
                .OrderBy(t => t.DueAt).Take(8).ToListAsync(),
            RecentDisputes = await _db.Disputes
                .Include(d => d.Tool).Include(d => d.ReportedByUser)
                .OrderByDescending(d => d.CreatedAt).Take(5).ToListAsync(),
        };
        return dash;
    }

    public async Task<List<LibrarianTask>> GetMyTasksAsync(int librarianUserId) =>
        await _db.LibrarianTasks
            .Include(t => t.Zone)
            .Where(t => t.LibrarianUserId == librarianUserId)
            .OrderBy(t => t.Status == "Completed").ThenBy(t => t.DueAt)
            .ToListAsync();

    public async Task<bool> CompleteTaskAsync(int taskId, int librarianUserId)
    {
        var task = await _db.LibrarianTasks.FindAsync(taskId);
        if (task is null || task.LibrarianUserId != librarianUserId) return false;
        task.Status = "Completed";
        task.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    /* ---------------- disputes queue ---------------- */

    public async Task<List<Dispute>> GetDisputesAsync(string? status)
    {
        var q = _db.Disputes
            .Include(d => d.Tool).Include(d => d.ReportedByUser)
            .Include(d => d.AssignedLibrarianUser)
            .AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<DisputeStatusEnum>(status, out var disputeStatus))
            q = q.Where(d => d.Status == disputeStatus);
        return await q.OrderByDescending(d => d.CreatedAt).ToListAsync();
    }

    public async Task<Dispute?> GetDisputeForReviewAsync(int disputeId, int librarianUserId)
    {
        var d = await _db.Disputes
            .Include(x => x.Tool).Include(x => x.ReportedByUser).Include(x => x.AgainstUser)
            .Include(x => x.AssignedLibrarianUser).Include(x => x.Booking)
            .FirstOrDefaultAsync(x => x.DisputeId == disputeId);
        if (d is null) return null;
        if (d.AssignedLibrarianUserId == null)
        {
            d.AssignedLibrarianUserId = librarianUserId;
            if (d.Status == DisputeStatusEnum.Open) d.Status = DisputeStatusEnum.InReview;
            await _db.SaveChangesAsync();
        }
        return d;
    }

    /* ---------------- inventory audits ---------------- */

    public async Task<List<InventoryAudit>> GetAuditsAsync(string? status)
    {
        var q = _db.InventoryAudits.Include(a => a.Tool).Include(a => a.OwnerUser).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<AuditStatusEnum>(status, out var auditStatus))
            q = q.Where(a => a.Status == auditStatus);
        return await q.OrderBy(a => a.DueDate).ToListAsync();
    }

    public async Task<bool> ReviewAuditAsync(int auditId, string decision, string? notes, int reviewerUserId)
    {
        var a = await _db.InventoryAudits.FindAsync(auditId);
        if (a is null) return false;
        a.ReviewDecision = decision;
        a.ReviewNotes = notes;
        a.ReviewedByUserId = reviewerUserId;
        a.ReviewedAt = DateTime.UtcNow;
        a.Status = decision switch
        {
            "Approve" => AuditStatusEnum.Approved,
            "Escalate" => AuditStatusEnum.Failed,
            _ => AuditStatusEnum.Rejected,
        };
        await _db.SaveChangesAsync();
        return true;
    }

    /* ---------------- handovers ---------------- */

    public async Task<List<Handover>> GetHandoversAsync() =>
        await _db.Handovers
            .Include(h => h.Tool).Include(h => h.Booking)
            .Include(h => h.BorrowerUser).Include(h => h.LenderUser)
            .OrderByDescending(h => h.ScheduledAt).Take(100).ToListAsync();

    public async Task StampHandoverVerifierAsync(int handoverId, int staffUserId)
    {
        var h = await _db.Handovers.FindAsync(handoverId);
        if (h is not null)
        {
            h.VerifiedByUserId = staffUserId;
            await _db.SaveChangesAsync();
        }
    }

    /* ---------------- insurance claims ---------------- */

    public async Task<List<InsuranceClaim>> GetClaimsAsync(string? status)
    {
        var q = _db.InsuranceClaims.Include(c => c.Tool).Include(c => c.ClaimantUser).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<InsuranceClaimStatusEnum>(status, out var claimStatus))
            q = q.Where(c => c.Status == claimStatus);
        return await q.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task ReviewClaimAsync(int claimId, bool approve, string? notes, decimal? payout)
    {
        var c = await _db.InsuranceClaims.FindAsync(claimId);
        if (c is null) return;
        var now = DateTime.UtcNow;
        c.Status = approve ? InsuranceClaimStatusEnum.Approved : InsuranceClaimStatusEnum.Rejected;
        c.DecisionNotes = notes;
        c.PayoutAmount = payout;
        c.DecidedAt = now;
        if (c.Status == InsuranceClaimStatusEnum.Open) c.UnderReviewAt = now;

        if (approve && payout is > 0)
        {
            var claimant = await _db.Users.FindAsync(c.ClaimantUserId);
            if (claimant is not null)
            {
                claimant.WalletBalance += payout.Value;
                _db.Transactions.Add(new Transaction
                {
                    UserId = claimant.UserId, BookingId = c.BookingId,
                    Type = TransactionTypeEnum.Refund, Amount = payout.Value, Status = TransactionStatusEnum.Completed,
                    Description = $"Insurance payout — claim #{c.ClaimId}",
                    CreatedAt = now,
                });
            }
        }
        await _db.SaveChangesAsync();
    }

    /* ---------------- late returns ---------------- */

    public async Task<List<LateReturn>> GetLateReturnsAsync(string? status)
    {
        var q = _db.LateReturns.Include(l => l.Tool).Include(l => l.BorrowerUser).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<LateReturnStatusEnum>(status, out var lateStatus))
            q = q.Where(l => l.Status == lateStatus);
        return await q.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task ResolveLateReturnAsync(int lateReturnId)
    {
        var l = await _db.LateReturns.FindAsync(lateReturnId);
        if (l is null) return;
        l.Status = LateReturnStatusEnum.Resolved;
        l.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /* ---------------- broadcasts ---------------- */

    public async Task<List<Broadcast>> GetBroadcastsAsync() =>
        await _db.Broadcasts
            .Include(b => b.Zone).Include(b => b.SentByUser)
            .OrderByDescending(b => b.SentAt).ToListAsync();

    public async Task<int> SendBroadcastAsync(int? zoneId, string title, string message, string priority, int senderUserId)
    {
        var recipients = await _db.Users.CountAsync(u => u.IsActive && (!zoneId.HasValue || u.ZoneId == zoneId));
        _db.Broadcasts.Add(new Broadcast
        {
            ZoneId = zoneId,
            Title = title,
            Message = message,
            Priority = priority,
            RecipientCount = recipients,
            SentByUserId = senderUserId,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        });
        var targets = _db.Users.Where(u => u.IsActive && (!zoneId.HasValue || u.ZoneId == zoneId)).Select(u => u.UserId);
        var notifPriority = Enum.TryParse<NotificationPriorityEnum>(priority, out var np) && np == NotificationPriorityEnum.Emergency
            ? NotificationPriorityEnum.Emergency : NotificationPriorityEnum.Normal;
        var now = DateTime.UtcNow;
        foreach (var uid in await targets.ToListAsync())
        {
            _db.Notifications.Add(new Notification
            {
                UserId = uid, Type = "System", Title = title, Message = message,
                Priority = notifPriority,
                CreatedAt = now,
            });
        }
        await _db.SaveChangesAsync();
        return recipients;
    }

    public async Task ResolveDisputeAsync(int disputeId, DisputeResolutionEnum resolution, string? notes, decimal? refundAmount)
    {
        var d = await _db.Disputes.Include(x => x.Booking).FirstOrDefaultAsync(x => x.DisputeId == disputeId);
        if (d is null) return;

        d.Resolution = resolution;
        d.ResolutionNotes = notes;
        d.RefundAmount = refundAmount;
        d.Status = DisputeStatusEnum.Resolved;
        d.ResolvedAt = DateTime.UtcNow;
        d.UpdatedAt = DateTime.UtcNow;

        if (d.Booking is not null)
        {
            var deposit = d.DepositAtRisk ?? d.Booking.DepositAmount;
            var borrower = await _db.Users.FindAsync(d.Booking.BorrowerUserId);
            var lender = await _db.Users.FindAsync(d.Booking.LenderUserId);
            var now = DateTime.UtcNow;

            async Task Payout(int userId, User? user, decimal amount, TransactionTypeEnum type, string desc)
            {
                if (amount <= 0 || user is null) return;
                user.WalletBalance += amount;
                _db.Transactions.Add(new Transaction
                {
                    UserId = userId, BookingId = d.BookingId, Type = type, Amount = amount,
                    BalanceAfter = user.WalletBalance, Status = TransactionStatusEnum.Completed,
                    Description = desc, Reference = $"DSP-{d.DisputeId}", CreatedAt = now,
                });
            }

            switch (resolution)
            {
                case DisputeResolutionEnum.ReleaseDepositToLender:
                    await Payout(d.Booking.LenderUserId, lender, deposit, TransactionTypeEnum.Credit, $"Deposit released to lender (dispute #{d.DisputeId})");
                    break;
                case DisputeResolutionEnum.FullRefundToBorrower:
                    await Payout(d.Booking.BorrowerUserId, borrower, deposit, TransactionTypeEnum.Refund, $"Deposit refunded to borrower (dispute #{d.DisputeId})");
                    break;
                case DisputeResolutionEnum.PartialRefundToBorrower:
                case DisputeResolutionEnum.SplitDeposit:
                    var borrowerShare = resolution == DisputeResolutionEnum.SplitDeposit
                        ? (refundAmount ?? deposit / 2)
                        : (refundAmount ?? 0);
                    borrowerShare = Math.Clamp(borrowerShare, 0, deposit);
                    var lenderShare = deposit - borrowerShare;
                    await Payout(d.Booking.BorrowerUserId, borrower, borrowerShare, TransactionTypeEnum.Refund, $"Partial deposit refund (dispute #{d.DisputeId})");
                    await Payout(d.Booking.LenderUserId, lender, lenderShare, TransactionTypeEnum.Credit, $"Deposit share to lender (dispute #{d.DisputeId})");
                    break;
            }

            d.Booking.DepositStatus = DepositStatusEnum.Released;
            d.Booking.UpdatedAt = now;
        }

        _db.Notifications.Add(new Notification
        {
            UserId = d.ReportedByUserId, Type = "Dispute", Title = "Dispute resolved",
            Message = $"Your dispute #{d.DisputeId} was resolved: {resolution}.",
            Priority = NotificationPriorityEnum.High, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    private async Task<string?> SaveHandoverPhoto(IFormFile? photo, string prefix)
    {
        if (photo is null || photo.Length == 0 || photo.Length > 8 * 1024 * 1024) return null;
        var ext = Path.GetExtension(photo.FileName);
        if (!HandoverPhotoExts.Contains(ext)) return null;
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "handovers");
        Directory.CreateDirectory(dir);
        var fname = $"{prefix}-{Guid.NewGuid():N}{ext}";
        await using (var fs = System.IO.File.Create(Path.Combine(dir, fname)))
            await photo.CopyToAsync(fs);
        return $"/uploads/handovers/{fname}";
    }
}
