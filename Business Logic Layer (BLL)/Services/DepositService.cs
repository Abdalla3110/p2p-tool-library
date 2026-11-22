using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class DepositService : IDepositService
{
    private readonly AppDbContext _db;
    public DepositService(AppDbContext db) => _db = db;

    public async Task<Deposit?> GetForBookingAsync(int bookingId) =>
        await _db.Deposits.FirstOrDefaultAsync(d => d.BookingId == bookingId);

    /// <summary>Release held deposit to the lender's wallet (successful return / dispute resolution).</summary>
    public async Task ReleaseToLenderAsync(int bookingId)
    {
        var booking = await _db.Bookings.Include(b => b.Deposit).FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking?.Deposit is null || booking.Deposit.Status != DepositStatusEnum.Held) return;
        var lender = await _db.Users.FindAsync(booking.LenderUserId);
        if (lender is not null)
        {
            lender.WalletBalance += booking.Deposit.Amount;
            _db.Transactions.Add(new Transaction
            {
                UserId = lender.UserId, BookingId = bookingId, Type = TransactionTypeEnum.Credit,
                Amount = booking.Deposit.Amount, BalanceAfter = lender.WalletBalance,
                Status = TransactionStatusEnum.Completed,
                Description = $"Deposit released — booking #{bookingId}",
                Reference = $"DEP-{bookingId}", CreatedAt = DateTime.UtcNow,
            });
        }
        booking.Deposit.Status = DepositStatusEnum.Released;
        booking.Deposit.ReleasedAt = DateTime.UtcNow;
        booking.DepositStatus = DepositStatusEnum.Released;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Refund deposit (or part) to the borrower's wallet.</summary>
    public async Task RefundToBorrowerAsync(int bookingId, decimal amount)
    {
        var booking = await _db.Bookings.Include(b => b.Deposit).FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking?.Deposit is null) return;
        var borrower = await _db.Users.FindAsync(booking.BorrowerUserId);
        if (borrower is not null && amount > 0)
        {
            borrower.WalletBalance += amount;
            _db.Transactions.Add(new Transaction
            {
                UserId = borrower.UserId, BookingId = bookingId, Type = TransactionTypeEnum.Refund,
                Amount = amount, BalanceAfter = borrower.WalletBalance,
                Status = TransactionStatusEnum.Completed,
                Description = $"Deposit refund — booking #{bookingId}",
                Reference = $"DEP-{bookingId}", CreatedAt = DateTime.UtcNow,
            });
        }
        booking.Deposit.Status = DepositStatusEnum.Refunded;
        booking.Deposit.ReleasedAt = DateTime.UtcNow;
        booking.DepositStatus = DepositStatusEnum.Refunded;
        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Split deposit between borrower and lender per dispute resolution.</summary>
    public async Task SplitAsync(int bookingId, decimal borrowerAmount, decimal lenderAmount)
    {
        var booking = await _db.Bookings.Include(b => b.Deposit).FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking?.Deposit is null) return;
        var now = DateTime.UtcNow;
        var borrower = await _db.Users.FindAsync(booking.BorrowerUserId);
        var lender = await _db.Users.FindAsync(booking.LenderUserId);
        if (borrower is not null && borrowerAmount > 0)
        {
            borrower.WalletBalance += borrowerAmount;
            _db.Transactions.Add(new Transaction
            {
                UserId = borrower.UserId, BookingId = bookingId, Type = TransactionTypeEnum.Refund,
                Amount = borrowerAmount, BalanceAfter = borrower.WalletBalance,
                Status = TransactionStatusEnum.Completed,
                Description = $"Deposit split — borrower share, booking #{bookingId}",
                Reference = $"DEP-{bookingId}", CreatedAt = now,
            });
        }
        if (lender is not null && lenderAmount > 0)
        {
            lender.WalletBalance += lenderAmount;
            _db.Transactions.Add(new Transaction
            {
                UserId = lender.UserId, BookingId = bookingId, Type = TransactionTypeEnum.Credit,
                Amount = lenderAmount, BalanceAfter = lender.WalletBalance,
                Status = TransactionStatusEnum.Completed,
                Description = $"Deposit split — lender share, booking #{bookingId}",
                Reference = $"DEP-{bookingId}", CreatedAt = now,
            });
        }
        booking.Deposit.Status = DepositStatusEnum.Released;
        booking.Deposit.ReleasedAt = now;
        booking.DepositStatus = DepositStatusEnum.Released;
        booking.UpdatedAt = now;
        await _db.SaveChangesAsync();
    }
}
