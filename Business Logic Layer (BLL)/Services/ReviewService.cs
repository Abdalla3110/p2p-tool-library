using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;
    public ReviewService(AppDbContext db) => _db = db;

    public async Task<(bool Ok, string? Error)> RateToolAsync(int toolId, int raterId, int rating, string? comment)
    {
        // toolId here is actually the booking id in the UI flow — kept for signature clarity
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == toolId);
        if (booking is null) return (false, "NOT_FOUND");
        if (booking.Status != BookingStatusEnum.Completed || rating < 1 || rating > 5)
            return (false, "BAD_STATE");

        var exists = await _db.Ratings.AnyAsync(r =>
            r.BookingId == booking.BookingId && r.ToolId == booking.ToolId &&
            r.RaterUserId == raterId && r.RatedUserId == null);
        if (exists) return (true, null);

        _db.Ratings.Add(new Rating
        {
            BookingId = booking.BookingId, ToolId = booking.ToolId, RaterUserId = raterId,
            RatedUserId = null, Rating1 = rating, Comment = comment,
            CreatedAt = DateTime.UtcNow,
        });
        var tool = await _db.Tools.FindAsync(booking.ToolId);
        if (tool is not null)
        {
            var all = await _db.Ratings.Where(r => r.ToolId == booking.ToolId && r.RatedUserId == null)
                .Select(r => r.Rating1).ToListAsync();
            all.Add(rating);
            tool.ReviewCount = all.Count;
            tool.RatingAverage = (decimal)Math.Round(all.Average(), 2);
        }
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> RateBookingAsync(int bookingId, int raterId, int rating, string? comment)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking is null) return (false, "NOT_FOUND");
        if (booking.Status != BookingStatusEnum.Completed) return (false, "NOT_COMPLETED");
        if (raterId != booking.BorrowerUserId && raterId != booking.LenderUserId) return (false, "FORBIDDEN");
        if (rating < 1 || rating > 5) return (false, "BAD_RATING");

        var ratedUserId = raterId == booking.BorrowerUserId ? booking.LenderUserId : booking.BorrowerUserId;
        var exists = await _db.Ratings.AnyAsync(r =>
            r.BookingId == bookingId && r.RaterUserId == raterId && r.RatedUserId == ratedUserId);
        if (exists) return (true, null);

        _db.Ratings.Add(new Rating
        {
            BookingId = bookingId, ToolId = booking.ToolId, RaterUserId = raterId,
            RatedUserId = ratedUserId, Rating1 = rating, Comment = comment,
            CreatedAt = DateTime.UtcNow,
        });

        var rated = await _db.Users.FindAsync(ratedUserId);
        if (rated is not null)
        {
            var all = await _db.Ratings.Where(r => r.RatedUserId == ratedUserId).Select(r => r.Rating1).ToListAsync();
            all.Add(rating);
            rated.ReviewCount = all.Count;
            rated.RatingAverage = (decimal)Math.Round(all.Average(), 2);
            rated.TrustScore = Math.Clamp((int)(rated.RatingAverage * 20), 0, 100);
        }
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<Rating>> GetReceivedAsync(int userId) =>
        await _db.Ratings.Include(r => r.RaterUser).Include(r => r.Booking)
            .Where(r => r.RatedUserId == userId).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<List<Rating>> GetGivenAsync(int userId) =>
        await _db.Ratings.Include(r => r.RatedUser).Include(r => r.Booking)
            .Where(r => r.RaterUserId == userId).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<bool> HasRatedBookingAsync(int bookingId, int raterId, int ratedUserId) =>
        await _db.Ratings.AnyAsync(r =>
            r.BookingId == bookingId && r.RaterUserId == raterId && r.RatedUserId == ratedUserId);
}
