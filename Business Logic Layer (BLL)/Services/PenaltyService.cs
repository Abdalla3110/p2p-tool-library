using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

/// <summary>
/// Late-return escalation logic: Warning → FineApplied (wallet debit) → AccountFrozen.
/// Called periodically by LateReturnJob.
/// </summary>
public class PenaltyService : IPenaltyService
{
    private readonly AppDbContext _db;
    public PenaltyService(AppDbContext db) => _db = db;

    public async Task ProcessLateReturnsAsync()
    {
        var settings = await _db.PlatformSettings.FirstOrDefaultAsync();
        var graceHours = settings?.LateReturnGraceHours ?? 24;
        var penaltyPerDay = settings?.LateReturnPenaltyPerDay ?? 50m;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var overdue = await _db.Bookings
            .Include(b => b.Tool)
            .Where(b => b.Status == BookingStatusEnum.Active && b.ReturnDate < today)
            .ToListAsync();

        foreach (var b in overdue)
        {
            var daysLate = Math.Abs(b.ReturnDate.DayNumber - today.DayNumber);
            var dueDateTime = b.ReturnDate.ToDateTime(TimeOnly.MinValue).AddHours(graceHours);
            if (DateTime.UtcNow < dueDateTime) continue;

            var existing = await _db.LateReturns
                .Where(l => l.BookingId == b.BookingId)
                .OrderByDescending(l => l.LateReturnId).FirstOrDefaultAsync();

            var penalty = Math.Round(daysLate * penaltyPerDay, 2);
            var now = DateTime.UtcNow;

            if (existing is null)
            {
                _db.LateReturns.Add(new LateReturn
                {
                    BookingId = b.BookingId, ToolId = b.ToolId, BorrowerUserId = b.BorrowerUserId,
                    DueDate = b.ReturnDate, DelayDays = daysLate, GraceHoursUsed = graceHours,
                    PenaltyAmount = penalty, Status = LateReturnStatusEnum.Warning, WarningSent = true,
                    WarningSentAt = now, CreatedAt = now,
                });
                Notify(b.BorrowerUserId, "LateReturn", "Late return warning",
                    $"Booking #{b.BookingId} ({b.Tool?.Name}) is {daysLate} day(s) overdue. Return it or a fine of {penalty:0.00} EGP will apply.", now);
            }
            else if (existing.Status == LateReturnStatusEnum.Warning && daysLate >= 3)
            {
                existing.Status = LateReturnStatusEnum.FineApplied;
                existing.DelayDays = daysLate;
                existing.PenaltyAmount = penalty;
                var borrower = await _db.Users.FindAsync(b.BorrowerUserId);
                if (borrower is not null)
                {
                    borrower.WalletBalance -= penalty;
                    _db.Transactions.Add(new Transaction
                    {
                        UserId = borrower.UserId, BookingId = b.BookingId, Type = TransactionTypeEnum.Fine,
                        Amount = -penalty, BalanceAfter = borrower.WalletBalance, Status = TransactionStatusEnum.Completed,
                        Description = $"Late return fine — booking #{b.BookingId} ({daysLate} days)",
                        Reference = $"LR-{b.BookingId}", CreatedAt = now,
                    });
                }
                Notify(b.BorrowerUserId, "LateReturn", "Late return fine applied",
                    $"A fine of {penalty:0.00} EGP was deducted for booking #{b.BookingId}.", now);
            }
            else if (existing.Status == LateReturnStatusEnum.FineApplied && daysLate >= 7 && !existing.AccountFrozen)
            {
                existing.Status = LateReturnStatusEnum.AccountFrozen;
                existing.AccountFrozen = true;
                existing.FrozenAt = now;
                var borrower = await _db.Users.FindAsync(b.BorrowerUserId);
                if (borrower is not null) borrower.IsActive = false;
                Notify(b.BorrowerUserId, "LateReturn", "Account frozen",
                    $"Your account was frozen for booking #{b.BookingId} overdue by {daysLate} days.", now);
                Notify(b.LenderUserId, "LateReturn", "Borrower account frozen",
                    $"The borrower on booking #{b.BookingId} was frozen for a {daysLate}-day late return.", now);
            }

            await _db.SaveChangesAsync();
        }
    }

    private void Notify(int userId, string type, string title, string msg, DateTime now)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId, Type = type, Title = title, Message = msg,
            Priority = NotificationPriorityEnum.High, CreatedAt = now,
        });
    }
}
