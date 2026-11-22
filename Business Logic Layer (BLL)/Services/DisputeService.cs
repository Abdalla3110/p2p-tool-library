using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

using BLL.Interfaces;

namespace BLL.Services;

/// <summary>Opens disputes and routes them to the zone librarian.</summary>
public class DisputeService : IDisputeService
{
    private readonly AppDbContext _db;

    public DisputeService(AppDbContext db) => _db = db;

    /// <summary>
    /// Creates a dispute for a booking, marks the booking Disputed,
    /// freezes the deposit, notifies the counterparty and the zone librarian,
    /// and creates a librarian task.
    /// </summary>
    public async Task<Dispute> OpenDisputeAsync(
        int bookingId, int reportedByUserId, DisputeTypeEnum type, string description,
        int? againstUserId = null, string? damagePhotoUrl = null)
    {
        var booking = await _db.Bookings
            .Include(b => b.Tool).ThenInclude(t => t.Zone)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId)
            ?? throw new InvalidOperationException("Booking not found.");

        var counterpartyId = reportedByUserId == booking.BorrowerUserId
            ? booking.LenderUserId : booking.BorrowerUserId;

        var now = DateTime.UtcNow;
        var dispute = new Dispute
        {
            BookingId = bookingId,
            ToolId = booking.ToolId,
            ReportedByUserId = reportedByUserId,
            AgainstUserId = againstUserId ?? counterpartyId,
            AgainstType = "User",
            Type = type,
            Description = description,
            DamagePhotoUrl = damagePhotoUrl,
            DepositAtRisk = booking.DepositAmount,
            Status = DisputeStatusEnum.Open,
            OpenedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _db.Disputes.Add(dispute);

        if (booking.Status is BookingStatusEnum.Active or BookingStatusEnum.Completed or BookingStatusEnum.Confirmed)
        {
            booking.Status = BookingStatusEnum.Disputed;
            booking.UpdatedAt = now;
        }

        // notify counterparty
        _db.Notifications.Add(new Notification
        {
            UserId = counterpartyId,
            Type = "Dispute",
            Title = "Dispute opened",
            Message = $"A {type} dispute was opened on booking #{bookingId} ({booking.Tool?.Name}).",
            Priority = NotificationPriorityEnum.High,
            CreatedAt = now,
        });

        // route to zone librarian (fallback: any active librarian) + task
        var librarianId = booking.Tool?.Zone?.LibrarianUserId
            ?? await _db.Users.Where(u => u.Role!.Name == "Librarian" && u.IsActive)
                .Select(u => (int?)u.UserId).FirstOrDefaultAsync();
        if (librarianId.HasValue)
        {
            dispute.AssignedLibrarianUserId = librarianId;
            _db.LibrarianTasks.Add(new LibrarianTask
            {
                LibrarianUserId = librarianId.Value,
                ZoneId = booking.Tool?.ZoneId,
                Type = "DisputeReview",
                RelatedEntityType = "Dispute",
                RelatedEntityId = bookingId.ToString(),
                Title = $"Dispute #{bookingId} — {type}",
                Description = $"{type} dispute on {booking.Tool?.Name} reported by user {reportedByUserId}.",
                DueAt = now.AddDays(2),
                Status = "Pending",
                Priority = "High",
                CreatedAt = now,
            });
            _db.Notifications.Add(new Notification
            {
                UserId = librarianId.Value,
                Type = "Dispute",
                Title = "New dispute assigned",
                Message = $"Dispute #{bookingId} ({type}) needs your review.",
                Priority = NotificationPriorityEnum.High,
                CreatedAt = now,
            });
        }

        await _db.SaveChangesAsync();
        return dispute;
    }

    public async Task<List<Dispute>> GetMyDisputesAsync(int userId) =>
        await _db.Disputes
            .Include(d => d.Tool).Include(d => d.Booking)
            .Where(d => d.ReportedByUserId == userId || d.AgainstUserId == userId)
            .OrderByDescending(d => d.CreatedAt).ToListAsync();
}
