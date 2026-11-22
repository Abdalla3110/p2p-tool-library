using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

/// <summary>Creates damage reports and auto-opens a linked dispute for librarian review.</summary>
public class DamageReportService : IDamageReportService
{
    private readonly AppDbContext _db;
    private readonly IDisputeService _disputes;

    public DamageReportService(AppDbContext db, IDisputeService disputes)
    {
        _db = db;
        _disputes = disputes;
    }

    public async Task<DamageReportResult> ReportAsync(int bookingId, int userId, bool isStaff,
        string? description, string? photoUrl)
    {
        var booking = await _db.Bookings.Include(b => b.Tool)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking is null) return new DamageReportResult { Error = "NOT_FOUND" };
        if (!isStaff && userId != booking.BorrowerUserId && userId != booking.LenderUserId)
            return new DamageReportResult { Error = "FORBIDDEN" };

        var report = new DamageReport
        {
            BookingId = booking.BookingId,
            ToolId = booking.ToolId,
            ReportedByUserId = userId,
            Description = description,
            PhotoUrl = photoUrl,
            Status = DamageReportStatusEnum.Submitted,
            CreatedAt = DateTime.UtcNow,
        };
        _db.DamageReports.Add(report);
        await _db.SaveChangesAsync();

        var otherId = userId == booking.BorrowerUserId ? booking.LenderUserId : booking.BorrowerUserId;
        var dispute = await _disputes.OpenDisputeAsync(
            booking.BookingId, userId, DisputeTypeEnum.Damage,
            description ?? "Damage reported during rental.",
            againstUserId: otherId,
            damagePhotoUrl: photoUrl);
        report.DisputeId = dispute.DisputeId;
        report.Status = DamageReportStatusEnum.LinkedToDispute;
        await _db.SaveChangesAsync();

        return new DamageReportResult { Success = true };
    }
}
