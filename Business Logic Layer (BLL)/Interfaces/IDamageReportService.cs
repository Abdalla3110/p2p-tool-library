using DAL.Models.Entities;
using DAL.Models.ViewModels;

namespace BLL.Interfaces;

public class DamageReportResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public interface IDamageReportService
{
    Task<DamageReportResult> ReportAsync(int bookingId, int userId, bool isStaff, string? description, string? photoUrl);
}
