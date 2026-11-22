using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _db;
    public ReservationService(AppDbContext db) => _db = db;

    public async Task<BookingResult> CreateBookingAsync(int toolId, int borrowerId,
        DateTime pickupDate, DateTime returnDate, Func<int, string> bookingUrlFactory)
    {
        var tool = await _db.Tools.Include(t => t.OwnerUser)
            .FirstOrDefaultAsync(t => t.ToolId == toolId && t.IsActive);
        if (tool is null) return new BookingResult { Error = "NOT_FOUND" };

        var paused = await _db.PlatformSettings.Where(s => s.SettingsId == 1)
            .Select(s => s.PlatformPaused).FirstOrDefaultAsync();
        if (paused) return new BookingResult { Error = "PAUSED" };

        var from = DateOnly.FromDateTime(pickupDate);
        var to = DateOnly.FromDateTime(returnDate);

        if (borrowerId == tool.OwnerUserId) return new BookingResult { Error = "OWN_TOOL" };
        if (to <= from) return new BookingResult { Error = "BAD_DATES" };
        if (to.DayNumber - from.DayNumber > tool.MaxBorrowDays)
            return new BookingResult { Error = $"MAX_DAYS:{tool.MaxBorrowDays}" };

        var hasConflict = await _db.Bookings.AnyAsync(b =>
            b.ToolId == toolId && b.Status != BookingStatusEnum.Cancelled &&
            b.PickupDate < to && b.ReturnDate > from);
        if (hasConflict) return new BookingResult { Error = "CONFLICT" };

        var blocked = await _db.ToolAvailabilities.AnyAsync(a =>
            a.ToolId == toolId && a.Date >= from && a.Date < to && a.Status == AvailabilityStatusEnum.Unavailable);
        if (blocked) return new BookingResult { Error = "UNAVAILABLE" };

        var days = to.DayNumber - from.DayNumber;
        var rental = tool.PricePerWeek.HasValue && days >= 7
            ? tool.PricePerWeek.Value * (int)Math.Ceiling(days / 7.0)
            : tool.PricePerDay * days;
        var insurance = Math.Round(rental * tool.InsurancePercentage / 100m, 2);

        var settings = await _db.PlatformSettings.FirstOrDefaultAsync();
        var commissionPct = settings?.PlatformCommissionPercentage ?? 18m;
        var platformFee = Math.Round(rental * commissionPct / 100m, 2);

        var booking = new Booking
        {
            ToolId = toolId,
            BorrowerUserId = borrowerId,
            LenderUserId = tool.OwnerUserId,
            PickupDate = from,
            ReturnDate = to,
            RentalAmount = rental,
            InsuranceAmount = insurance,
            PlatformFee = platformFee,
            DepositAmount = tool.DepositAmount,
            DepositStatus = DepositStatusEnum.Pending,
            TotalAmount = rental + insurance + platformFee + tool.DepositAmount,
            WalletCreditApplied = 0,
            FinalAmountDue = rental + insurance + platformFee,
            Status = BookingStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            for (var d = from; d < to; d = d.AddDays(1))
                _db.ToolAvailabilities.Add(new ToolAvailability { ToolId = toolId, Date = d, Status = AvailabilityStatusEnum.Booked, BookingId = booking.BookingId });
            var bufferDays = (int)Math.Ceiling(tool.CleaningBufferHours / 24.0);
            for (var i = 0; i < Math.Max(1, bufferDays); i++)
            {
                var d = to.AddDays(i);
                if (!_db.ToolAvailabilities.Local.Any(a => a.ToolId == toolId && a.Date == d))
                    _db.ToolAvailabilities.Add(new ToolAvailability { ToolId = toolId, Date = d, Status = AvailabilityStatusEnum.CleaningBuffer, BookingId = booking.BookingId });
            }

            _db.Notifications.Add(new Notification
            {
                UserId = tool.OwnerUserId,
                Type = "Booking",
                Title = "New booking request",
                Message = $"A member wants to rent {tool.Name} ({from:yyyy-MM-dd} → {to:yyyy-MM-dd}).",
                Priority = NotificationPriorityEnum.Normal,
                ActionUrl = bookingUrlFactory(booking.BookingId),
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            _db.ChangeTracker.Clear();
            return new BookingResult { Error = "RACE" };
        }

        return new BookingResult { Success = true, Booking = booking };
    }

    public async Task<BookingResult> PayBookingAsync(int bookingId, int userId, Func<int, string> bookingUrlFactory)
    {
        var booking = await _db.Bookings.Include(b => b.Tool).ThenInclude(t => t.Zone)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking is null) return new BookingResult { Error = "NOT_FOUND" };
        if (booking.BorrowerUserId != userId) return new BookingResult { Error = "FORBIDDEN" };
        if (booking.Status != BookingStatusEnum.Pending) return new BookingResult { Error = "NOT_PENDING", Booking = booking };

        var borrower = await _db.Users.FindAsync(booking.BorrowerUserId);
        var totalCharge = booking.FinalAmountDue + booking.DepositAmount;
        if (borrower is null || borrower.WalletBalance < totalCharge)
            return new BookingResult { Error = "INSUFFICIENT_FUNDS", Booking = booking };

        var now = DateTime.UtcNow;
        borrower.WalletBalance -= totalCharge;

        _db.Transactions.Add(new Transaction
        {
            UserId = borrower.UserId, BookingId = booking.BookingId,
            Type = TransactionTypeEnum.Payment, Amount = -booking.FinalAmountDue, Status = TransactionStatusEnum.Completed,
            Description = $"Rental payment — {booking.Tool?.Name} (booking #{booking.BookingId})",
            Reference = $"BK-{booking.BookingId}", CreatedAt = now,
        });
        _db.Transactions.Add(new Transaction
        {
            UserId = borrower.UserId, BookingId = booking.BookingId,
            Type = TransactionTypeEnum.Deposit, Amount = -booking.DepositAmount, Status = TransactionStatusEnum.Held,
            Description = $"Security deposit held in escrow — booking #{booking.BookingId}",
            Reference = $"DEP-{booking.BookingId}", CreatedAt = now,
        });

        var lender = await _db.Users.FindAsync(booking.LenderUserId);
        if (lender is not null)
        {
            var payout = booking.RentalAmount - booking.PlatformFee;
            lender.WalletBalance += payout;
            _db.Transactions.Add(new Transaction
            {
                UserId = lender.UserId, BookingId = booking.BookingId,
                Type = TransactionTypeEnum.Credit, Amount = payout, Status = TransactionStatusEnum.Completed,
                Description = $"Earnings — {booking.Tool?.Name} (booking #{booking.BookingId})",
                Reference = $"BK-{booking.BookingId}", CreatedAt = now,
            });
        }

        _db.Deposits.Add(new Deposit
        {
            BookingId = booking.BookingId, Amount = booking.DepositAmount,
            Status = DepositStatusEnum.Held, HeldAt = now,
        });

        var settings = await _db.PlatformSettings.FirstOrDefaultAsync();
        var autoApprove = settings?.BookingAutoApproveEnabled ?? true;
        booking.Status = autoApprove ? BookingStatusEnum.Confirmed : BookingStatusEnum.WaitingHandover;
        booking.DepositStatus = DepositStatusEnum.Held;
        booking.UpdatedAt = now;

        _db.Handovers.Add(new Handover
        {
            BookingId = booking.BookingId, ToolId = booking.ToolId,
            BorrowerUserId = booking.BorrowerUserId, LenderUserId = booking.LenderUserId,
            ScheduledAt = booking.PickupDate.ToDateTime(new TimeOnly(10, 0)),
            Location = booking.Tool?.Zone?.Name ?? "TBD",
            QrCode = $"HVR-{booking.BookingId}-PICKUP-{Guid.NewGuid():N}"[..24],
            Type = HandoverTypeEnum.Pickup, Status = HandoverStatusEnum.PendingQR, CreatedAt = now,
        });
        _db.Handovers.Add(new Handover
        {
            BookingId = booking.BookingId, ToolId = booking.ToolId,
            BorrowerUserId = booking.BorrowerUserId, LenderUserId = booking.LenderUserId,
            ScheduledAt = booking.ReturnDate.ToDateTime(new TimeOnly(10, 0)),
            Location = booking.Tool?.Zone?.Name ?? "TBD",
            QrCode = $"HVR-{booking.BookingId}-RETURN-{Guid.NewGuid():N}"[..24],
            Type = HandoverTypeEnum.Return, Status = HandoverStatusEnum.PendingQR, CreatedAt = now,
        });

        var url = bookingUrlFactory(booking.BookingId);
        _db.Notifications.Add(new Notification
        {
            UserId = booking.LenderUserId, Type = "Booking", Title = "Booking paid",
            Message = $"Booking #{booking.BookingId} for {booking.Tool?.Name} is paid and confirmed. Prepare for handover on {booking.PickupDate:yyyy-MM-dd}.",
            Priority = NotificationPriorityEnum.High, ActionUrl = url, CreatedAt = now,
        });
        _db.Notifications.Add(new Notification
        {
            UserId = booking.BorrowerUserId, Type = "Payment", Title = "Payment successful",
            Message = $"Booking #{booking.BookingId} confirmed. Your deposit of {booking.DepositAmount:0} EGP is held in escrow.",
            Priority = NotificationPriorityEnum.Normal, ActionUrl = url, CreatedAt = now,
        });

        await _db.SaveChangesAsync();
        return new BookingResult { Success = true, Booking = booking };
    }

    public async Task<BookingResult> CancelBookingAsync(int bookingId, int userId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking is null) return new BookingResult { Error = "NOT_FOUND" };
        if (booking.BorrowerUserId != userId) return new BookingResult { Error = "FORBIDDEN" };
        if (booking.Status is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed or BookingStatusEnum.WaitingHandover))
            return new BookingResult { Error = "CANNOT_CANCEL", Booking = booking };

        var wasPaid = booking.DepositStatus == DepositStatusEnum.Held ||
                      booking.Status is BookingStatusEnum.Confirmed or BookingStatusEnum.WaitingHandover;
        booking.Status = BookingStatusEnum.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        decimal refundAmount = 0, fee = 0;
        if (wasPaid)
        {
            var now = DateTime.UtcNow;
            var borrower = await _db.Users.FindAsync(booking.BorrowerUserId);
            var lender = await _db.Users.FindAsync(booking.LenderUserId);
            fee = Math.Round(booking.RentalAmount * 0.10m, 2);
            refundAmount = booking.FinalAmountDue + booking.DepositAmount - fee;

            if (borrower is not null)
            {
                borrower.WalletBalance += refundAmount;
                _db.Transactions.Add(new Transaction
                {
                    UserId = borrower.UserId, BookingId = booking.BookingId,
                    Type = TransactionTypeEnum.Refund, Amount = refundAmount, BalanceAfter = borrower.WalletBalance,
                    Status = TransactionStatusEnum.Completed,
                    Description = $"Cancellation refund — booking #{booking.BookingId} (fee {fee:0.00} EGP)",
                    Reference = $"CAN-{booking.BookingId}", CreatedAt = now,
                });
            }

            var lenderCredit = booking.RentalAmount - booking.PlatformFee;
            if (lender is not null && lenderCredit > 0)
            {
                lender.WalletBalance -= lenderCredit;
                _db.Transactions.Add(new Transaction
                {
                    UserId = lender.UserId, BookingId = booking.BookingId,
                    Type = TransactionTypeEnum.Reversal, Amount = -lenderCredit, BalanceAfter = lender.WalletBalance,
                    Status = TransactionStatusEnum.Completed,
                    Description = $"Rental credit reversed — booking #{booking.BookingId} cancelled",
                    Reference = $"CAN-{booking.BookingId}", CreatedAt = now,
                });
            }

            booking.DepositStatus = DepositStatusEnum.Refunded;
            var dep = await _db.Deposits.FirstOrDefaultAsync(d => d.BookingId == booking.BookingId);
            if (dep is not null) dep.Status = DepositStatusEnum.Refunded;

            _db.Notifications.Add(new Notification
            {
                UserId = booking.BorrowerUserId, Type = "Booking", Title = "Booking cancelled — refund issued",
                Message = $"Booking #{booking.BookingId} cancelled. Refund of {refundAmount:0.00} EGP credited to your wallet (cancellation fee {fee:0.00} EGP).",
                Priority = NotificationPriorityEnum.Normal, CreatedAt = now,
            });
            _db.Notifications.Add(new Notification
            {
                UserId = booking.LenderUserId, Type = "Booking", Title = "Booking cancelled",
                Message = $"Booking #{booking.BookingId} was cancelled before handover. The rental credit was reversed.",
                Priority = NotificationPriorityEnum.Normal, CreatedAt = now,
            });
        }

        var slots = await _db.ToolAvailabilities
            .Where(a => a.BookingId == bookingId && (a.Status == AvailabilityStatusEnum.Booked || a.Status == AvailabilityStatusEnum.CleaningBuffer))
            .ToListAsync();
        _db.ToolAvailabilities.RemoveRange(slots);
        await _db.SaveChangesAsync();

        return new BookingResult { Success = true, Booking = booking, RefundAmount = refundAmount, FeeAmount = fee };
    }

    public async Task<BookingResult> RescheduleBookingAsync(int bookingId, int userId,
        DateTime pickupDate, DateTime returnDate)
    {
        var booking = await _db.Bookings.Include(b => b.Tool)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking is null) return new BookingResult { Error = "NOT_FOUND" };
        if (booking.BorrowerUserId != userId) return new BookingResult { Error = "FORBIDDEN" };
        if (booking.Status is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed or BookingStatusEnum.WaitingHandover or BookingStatusEnum.Active))
            return new BookingResult { Error = "CANNOT_RESCHEDULE", Booking = booking };

        var from = DateOnly.FromDateTime(pickupDate);
        var to = DateOnly.FromDateTime(returnDate);
        if (to <= from || (to.DayNumber - from.DayNumber) > (booking.Tool?.MaxBorrowDays ?? 0))
            return new BookingResult { Error = "BAD_DATES", Booking = booking };

        var conflict = await _db.ToolAvailabilities.AnyAsync(a =>
            a.ToolId == booking.ToolId && a.BookingId != bookingId &&
            a.Date >= from && a.Date < to &&
            (a.Status == AvailabilityStatusEnum.Booked || a.Status == AvailabilityStatusEnum.CleaningBuffer));
        if (conflict) return new BookingResult { Error = "CONFLICT", Booking = booking };

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var oldSlots = await _db.ToolAvailabilities.Where(a => a.BookingId == bookingId).ToListAsync();
            _db.ToolAvailabilities.RemoveRange(oldSlots);
            for (var d = from; d < to; d = d.AddDays(1))
                _db.ToolAvailabilities.Add(new ToolAvailability { ToolId = booking.ToolId, Date = d, Status = AvailabilityStatusEnum.Booked, BookingId = bookingId });
            var bufferDays = (int)Math.Ceiling((booking.Tool?.CleaningBufferHours ?? 10) / 24.0);
            for (var i = 0; i < Math.Max(1, bufferDays); i++)
                _db.ToolAvailabilities.Add(new ToolAvailability { ToolId = booking.ToolId, Date = to.AddDays(i), Status = AvailabilityStatusEnum.CleaningBuffer, BookingId = bookingId });

            booking.PickupDate = from;
            booking.ReturnDate = to;
            booking.UpdatedAt = DateTime.UtcNow;
            _db.Notifications.Add(new Notification
            {
                UserId = booking.LenderUserId, Type = "Booking", Title = "Booking rescheduled",
                Message = $"Booking #{bookingId} was rescheduled to {from:yyyy-MM-dd} → {to:yyyy-MM-dd}.",
                Priority = NotificationPriorityEnum.Normal, CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            _db.ChangeTracker.Clear();
            return new BookingResult { Error = "RACE", Booking = booking };
        }
        return new BookingResult { Success = true, Booking = booking };
    }

    public async Task<List<Booking>> GetMyBookingsAsync(int userId) =>
        await _db.Bookings
            .Include(b => b.Tool).ThenInclude(t => t.Category)
            .Include(b => b.Tool).ThenInclude(t => t.Zone)
            .Include(b => b.BorrowerUser)
            .Include(b => b.LenderUser)
            .Where(b => b.BorrowerUserId == userId || b.LenderUserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<Booking?> GetBookingWithPartiesAsync(int bookingId) =>
        await _db.Bookings
            .Include(b => b.Tool).ThenInclude(t => t.Category)
            .Include(b => b.Tool).ThenInclude(t => t.Zone)
            .Include(b => b.BorrowerUser).ThenInclude(u => u.Role)
            .Include(b => b.LenderUser).ThenInclude(u => u.Role)
            .Include(b => b.Handovers)
            .Include(b => b.Promotion)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

    public async Task<Booking?> GetBookingForCheckoutAsync(int bookingId) =>
        await _db.Bookings.Include(b => b.Tool).ThenInclude(t => t.Category)
            .Include(b => b.Promotion)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

    public async Task<Booking?> GetBookingForDisputeAsync(int bookingId) =>
        await _db.Bookings.Include(b => b.Tool).ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

    public async Task<Booking?> GetBookingForClaimAsync(int bookingId, bool includeZone)
    {
        var q = _db.Bookings.Include(b => b.Tool).AsQueryable();
        if (includeZone) q = q.Include(b => b.Tool).ThenInclude(t => t.Zone);
        return await q.FirstOrDefaultAsync(b => b.BookingId == bookingId);
    }

    public async Task<Booking?> GetBookingForDamageAsync(int bookingId) =>
        await _db.Bookings.Include(b => b.Tool)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

    public async Task<List<Booking>> GetAllBookingsAsync(string? status)
    {
        var q = _db.Bookings
            .Include(b => b.Tool).Include(b => b.BorrowerUser).Include(b => b.LenderUser)
            .AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<BookingStatusEnum>(status, out var bookingStatus))
            q = q.Where(b => b.Status == bookingStatus);
        return await q.OrderByDescending(b => b.CreatedAt).Take(200).ToListAsync();
    }
}
