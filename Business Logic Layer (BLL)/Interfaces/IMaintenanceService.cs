using DAL.Models.Entities;
using DAL.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BLL.Interfaces;

public class MaintenanceDashboard
{
    public int ToolsInMaintenance { get; set; }
    public int UnfitTools { get; set; }
    public int OpenRecords { get; set; }
    public int MyCompleted { get; set; }
    public int OpenRepairs { get; set; }
    public double AvgHours { get; set; }
    public List<MaintenanceRecord> MyRecords { get; set; } = new();
    public List<Tool> NeedsAttention { get; set; } = new();
}

public class TechnicianProfileView
{
    public User? User { get; set; }
    public TechnicianProfile? Profile { get; set; }
    public int TotalRepairs { get; set; }
    public double AvgHours { get; set; }
    public int OpenRepairs { get; set; }
    public int TotalToolsMaintained { get; set; }
}

public interface IMaintenanceService
{
    Task<MaintenanceRecord> CreateRecordAsync(MaintenanceRecord record);
    Task<bool> CompleteRecordAsync(int recordId, MaintenanceStatusEnum result, string? notes, int technicianId);
    Task<Tool?> GetToolHealthAsync(int toolId);
    Task<List<Tool>> GetToolsNeedingAttentionAsync();

    // dashboard + listings
    Task<MaintenanceDashboard> GetDashboardAsync(int technicianUserId);
    Task<List<Tool>> GetToolsForHealthAsync(string? status);
    Task<bool> SetToolStatusAsync(int toolId, string status);
    Task<List<MaintenanceRecord>> GetRecordsAsync(string? status);
    Task SaveRecordAsync(MaintenanceRecordFormViewModel m, int technicianUserId);
    Task<List<Tool>> GetActiveToolsAsync();
    Task<List<ExternalRepair>> GetRepairsAsync(string? status);
    Task SaveRepairAsync(ExternalRepairFormViewModel m, string? invoiceUrl);
    Task ReturnRepairAsync(int id, DateOnly? actualReturnAt, decimal? cost);
    Task<string?> SaveInvoiceAsync(IFormFile file);

    // disposals
    Task<List<Disposal>> GetDisposalsAsync();
    Task CreateDisposalAsync(int toolId, DisposalReasonEnum reason, string? reasonDescription, string? recycleCenter, bool cancelFutureBookings);
    Task ApproveDisposalAsync(int id, int approverUserId);

    // technician profile
    Task<TechnicianProfileView> GetTechnicianProfileAsync(int userId);
}
