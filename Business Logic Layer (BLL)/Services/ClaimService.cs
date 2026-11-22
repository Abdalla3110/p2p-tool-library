using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using DAL.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

/// <summary>Member-submitted insurance claims, routed to the zone librarian.</summary>
public class ClaimService : IClaimService
{
    private readonly AppDbContext _db;

    public ClaimService(AppDbContext db) => _db = db;

    public async Task<List<InsuranceClaim>> GetMyClaimsAsync(int userId) =>
        await _db.InsuranceClaims
            .Include(c => c.Tool).Include(c => c.Booking)
            .Where(c => c.ClaimantUserId == userId)
            .OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task<ClaimResult> SubmitAsync(InsuranceClaimFormViewModel m, int userId)
    {
        var booking = await _db.Bookings.Include(b => b.Tool).ThenInclude(t => t.Zone)
            .FirstOrDefaultAsync(b => b.BookingId == m.BookingId);
        if (booking is null) return new ClaimResult { Error = "NOT_FOUND" };
        if (userId != booking.BorrowerUserId && userId != booking.LenderUserId)
            return new ClaimResult { Error = "FORBIDDEN" };

        var now = DateTime.UtcNow;
        var claim = new InsuranceClaim
        {
            BookingId = m.BookingId,
            ToolId = m.ToolId,
            ClaimantUserId = userId,
            ToolValue = m.ToolValue,
            IncidentDate = m.IncidentDate,
            Description = m.Description,
            DamagePhotoUrl = m.DamagePhotoUrl,
            Status = InsuranceClaimStatusEnum.Open,
            SubmittedAt = now,
            CreatedAt = now,
        };
        _db.InsuranceClaims.Add(claim);

        var librarianId = booking.Tool?.Zone?.LibrarianUserId
            ?? await _db.Users.Where(u => u.Role!.Name == "Librarian" && u.IsActive)
                .Select(u => (int?)u.UserId).FirstOrDefaultAsync();
        if (librarianId.HasValue)
        {
            claim.AssignedLibrarianUserId = librarianId;
            _db.LibrarianTasks.Add(new LibrarianTask
            {
                LibrarianUserId = librarianId.Value,
                ZoneId = booking.Tool?.ZoneId,
                Type = "InsuranceClaimProcessing",
                RelatedEntityType = "InsuranceClaim",
                RelatedEntityId = m.BookingId.ToString(),
                Title = $"Insurance claim — booking #{m.BookingId}",
                Description = m.Description,
                DueAt = now.AddDays(3),
                Status = "Pending",
                Priority = "Normal",
                CreatedAt = now,
            });
            _db.Notifications.Add(new Notification
            {
                UserId = librarianId.Value, Type = "Insurance", Title = "New insurance claim",
                Message = $"Claim filed on booking #{m.BookingId} ({booking.Tool?.Name}).",
                Priority = NotificationPriorityEnum.Normal, CreatedAt = now,
            });
        }
        await _db.SaveChangesAsync();
        return new ClaimResult { Success = true };
    }
}
