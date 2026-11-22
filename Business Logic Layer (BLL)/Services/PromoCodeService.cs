using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly AppDbContext _db;
    public PromoCodeService(AppDbContext db) => _db = db;

    public async Task<PromoResult> ApplyAsync(int bookingId, string code, int userId)
    {
        var booking = await _db.Bookings.Include(b => b.Tool)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking is null) return new PromoResult { Error = "NOT_FOUND" };
        if (booking.BorrowerUserId != userId) return new PromoResult { Error = "FORBIDDEN" };
        if (booking.Status is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed))
            return new PromoResult { Error = "BAD_STATUS" };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var promo = await _db.Promotions.FirstOrDefaultAsync(p =>
            p.Code == code.Trim().ToUpper() && p.Status == PromotionStatusEnum.Active &&
            p.StartDate <= today && (p.EndDate == null || p.EndDate >= today) &&
            (p.MaxUses == null || p.UsedCount < p.MaxUses));
        if (promo is null) return new PromoResult { Error = "INVALID" };
        if (promo.CategoryId.HasValue && promo.CategoryId != booking.Tool?.CategoryId)
            return new PromoResult { Error = "WRONG_CATEGORY" };

        var discount = promo.DiscountType == DiscountTypeEnum.Percentage
            ? Math.Round(booking.RentalAmount * (promo.DiscountPercentage ?? 0) / 100m, 2)
            : Math.Min(promo.DiscountAmount ?? 0, booking.RentalAmount);
        discount = Math.Clamp(discount, 0, booking.RentalAmount);

        booking.PromotionId = promo.PromotionId;
        booking.PromotionDiscount = discount;
        booking.FinalAmountDue = booking.RentalAmount + booking.InsuranceAmount
            + booking.PlatformFee - discount;
        booking.TotalAmount = booking.RentalAmount + booking.InsuranceAmount
            + booking.PlatformFee + booking.DepositAmount - discount;
        booking.UpdatedAt = DateTime.UtcNow;
        promo.UsedCount++;
        await _db.SaveChangesAsync();
        return new PromoResult { Success = true, Discount = discount };
    }

    public async Task RemoveAsync(int bookingId, int userId)
    {
        var booking = await _db.Bookings.Include(b => b.Promotion)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking?.PromotionId is int pid && booking.BorrowerUserId == userId)
        {
            var promo = await _db.Promotions.FindAsync(pid);
            if (promo is not null && promo.UsedCount > 0) promo.UsedCount--;
            booking.PromotionId = null;
            booking.PromotionDiscount = 0;
            booking.FinalAmountDue = booking.RentalAmount + booking.InsuranceAmount + booking.PlatformFee;
            booking.TotalAmount = booking.RentalAmount + booking.InsuranceAmount
                + booking.PlatformFee + booking.DepositAmount;
            booking.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
    public async Task<List<Promotion>> GetAvailableAsync() =>
        await _db.Promotions
            .Where(p => p.Status == PromotionStatusEnum.Active
                && p.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow)
                && (p.EndDate == null || p.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                && (p.MaxUses == null || p.UsedCount < p.MaxUses))
            .OrderBy(p => p.Code).ToListAsync();
}
