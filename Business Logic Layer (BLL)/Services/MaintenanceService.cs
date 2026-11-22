using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using DAL.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly AppDbContext _db;
    public MaintenanceService(AppDbContext db) => _db = db;

    public async Task<MaintenanceRecord> CreateRecordAsync(MaintenanceRecord record)
    {
        _db.MaintenanceRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<bool> CompleteRecordAsync(int recordId, MaintenanceStatusEnum result, string? notes, int technicianId)
    {
        var rec = await _db.MaintenanceRecords.Include(r => r.Tool)
            .FirstOrDefaultAsync(r => r.MaintenanceId == recordId);
        if (rec is null) return false;

        rec.Status = result;
        rec.Notes = notes;
        rec.CompletedAt = DateTime.UtcNow;
        if (rec.Tool is not null)
        {
            rec.Tool.LastServiceAt = DateOnly.FromDateTime(DateTime.UtcNow);
            if (result is MaintenanceStatusEnum.Completed or MaintenanceStatusEnum.Passed)
                rec.Tool.Status = rec.Tool.Status == ToolStatusEnum.Maintenance ? ToolStatusEnum.Available : rec.Tool.Status;
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Tool?> GetToolHealthAsync(int toolId) =>
        await _db.Tools.Include(t => t.MaintenanceRecords.OrderByDescending(m => m.CreatedAt).Take(5))
            .FirstOrDefaultAsync(t => t.ToolId == toolId);

    public async Task<List<Tool>> GetToolsNeedingAttentionAsync() =>
        await _db.Tools.Where(t =>
                t.Status == ToolStatusEnum.Maintenance || t.Status == ToolStatusEnum.Unfit ||
                (t.BatteryHealthPercentage != null && t.BatteryHealthPercentage < 30) ||
                (t.LastServiceAt != null && t.LastServiceAt < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90))))
            .OrderBy(t => t.Status).ToListAsync();

    public async Task<MaintenanceDashboard> GetDashboardAsync(int technicianUserId)
    {
        var dash = new MaintenanceDashboard();
        dash.ToolsInMaintenance = await _db.Tools.CountAsync(t => t.Status == ToolStatusEnum.Maintenance);
        dash.UnfitTools = await _db.Tools.CountAsync(t => t.Status == ToolStatusEnum.Unfit);
        dash.OpenRecords = await _db.MaintenanceRecords.CountAsync(m =>
            m.Status != MaintenanceStatusEnum.Completed && m.Status != MaintenanceStatusEnum.Failed);
        dash.MyCompleted = await _db.MaintenanceRecords.CountAsync(m =>
            m.TechnicianUserId == technicianUserId && m.Status == MaintenanceStatusEnum.Completed);
        dash.OpenRepairs = await _db.ExternalRepairs.CountAsync(r =>
            r.Status != ExternalRepairStatusEnum.Completed && r.Status != ExternalRepairStatusEnum.Cancelled);
        dash.AvgHours = await _db.MaintenanceRecords
            .Where(m => m.TechnicianUserId == technicianUserId && m.Status == MaintenanceStatusEnum.Completed && m.DurationHours != null)
            .AverageAsync(m => (double?)m.DurationHours) ?? 0;
        dash.MyRecords = await _db.MaintenanceRecords
            .Include(m => m.Tool).Include(m => m.Tool.Category)
            .Where(m => m.TechnicianUserId == technicianUserId)
            .OrderByDescending(m => m.CreatedAt).Take(6).ToListAsync();
        dash.NeedsAttention = await _db.Tools
            .Include(t => t.Category).Include(t => t.Zone)
            .Where(t => t.Status == ToolStatusEnum.Maintenance || t.Status == ToolStatusEnum.Unfit ||
                        (t.BatteryHealthPercentage != null && t.BatteryHealthPercentage < 30))
            .OrderBy(t => t.Status).Take(8).ToListAsync();
        return dash;
    }

    public async Task<List<Tool>> GetToolsForHealthAsync(string? status)
    {
        var q = _db.Tools.Include(t => t.Category).Include(t => t.Zone).Include(t => t.OwnerUser).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<ToolStatusEnum>(status, out var toolStatus))
            q = q.Where(t => t.Status == toolStatus);
        return await q.OrderBy(t => t.Status).ThenBy(t => t.Name).Take(300).ToListAsync();
    }

    public async Task<bool> SetToolStatusAsync(int toolId, string status)
    {
        var t = await _db.Tools.FindAsync(toolId);
        if (t is null || !Enum.TryParse<ToolStatusEnum>(status, out var parsed)
            || parsed is not (ToolStatusEnum.Maintenance or ToolStatusEnum.Available or ToolStatusEnum.Unfit or ToolStatusEnum.Locked))
            return false;
        t.Status = parsed;
        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<MaintenanceRecord>> GetRecordsAsync(string? status)
    {
        var q = _db.MaintenanceRecords
            .Include(m => m.Tool).Include(m => m.Tool.Category)
            .Include(m => m.TechnicianUser)
            .AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<MaintenanceStatusEnum>(status, out var recStatus))
            q = q.Where(m => m.Status == recStatus);
        return await q.OrderByDescending(m => m.CreatedAt).Take(200).ToListAsync();
    }

    public async Task SaveRecordAsync(MaintenanceRecordFormViewModel m, int technicianUserId)
    {
        var now = DateTime.UtcNow;
        var completed = m.Status is MaintenanceStatusEnum.Completed or MaintenanceStatusEnum.Passed;
        _db.MaintenanceRecords.Add(new MaintenanceRecord
        {
            ToolId = m.ToolId,
            TechnicianUserId = technicianUserId,
            Type = m.Type,
            Description = m.Description,
            Status = m.Status,
            Notes = m.Notes,
            DurationHours = m.DurationHours,
            ScheduledAt = m.ScheduledAt,
            CompletedAt = completed ? now : null,
            CreatedAt = now,
        });

        var tool = await _db.Tools.FindAsync(m.ToolId);
        if (tool is not null)
        {
            tool.LastServiceAt = DateOnly.FromDateTime(DateTime.Today);
            tool.UpdatedAt = now;
            if (completed && tool.Status == ToolStatusEnum.Maintenance)
                tool.Status = ToolStatusEnum.Available;
        }
        await _db.SaveChangesAsync();

        var profile = await _db.TechnicianProfiles.FirstOrDefaultAsync(p => p.UserId == technicianUserId);
        if (profile is not null && completed)
        {
            profile.ToolsRepairedCount++;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<Tool>> GetActiveToolsAsync() =>
        await _db.Tools.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();

    public async Task<List<ExternalRepair>> GetRepairsAsync(string? status)
    {
        var q = _db.ExternalRepairs.Include(r => r.Tool).Include(r => r.Tool.Category).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<ExternalRepairStatusEnum>(status, out var repairStatus))
            q = q.Where(r => r.Status == repairStatus);
        return await q.OrderByDescending(r => r.CreatedAt).Take(200).ToListAsync();
    }

    public async Task SaveRepairAsync(ExternalRepairFormViewModel m, string? invoiceUrl)
    {
        _db.ExternalRepairs.Add(new ExternalRepair
        {
            ToolId = m.ToolId,
            RepairCenterName = m.RepairCenterName,
            ProblemDescription = m.ProblemDescription,
            SentAt = m.SentAt,
            ExpectedReturnAt = m.ExpectedReturnAt,
            Cost = m.Cost,
            InvoiceUrl = invoiceUrl,
            Status = m.Status,
            CreatedAt = DateTime.UtcNow,
        });

        if (m.Status == ExternalRepairStatusEnum.Inspecting)
        {
            var tool = await _db.Tools.FindAsync(m.ToolId);
            if (tool is not null && tool.Status == ToolStatusEnum.Available)
            {
                tool.Status = ToolStatusEnum.Maintenance;
                tool.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task ReturnRepairAsync(int id, DateOnly? actualReturnAt, decimal? cost)
    {
        var r = await _db.ExternalRepairs.Include(x => x.Tool).FirstOrDefaultAsync(x => x.ExternalRepairId == id);
        if (r is null) return;
        r.Status = ExternalRepairStatusEnum.Completed;
        r.ActualReturnAt = actualReturnAt ?? DateOnly.FromDateTime(DateTime.Today);
        if (cost.HasValue) r.Cost = cost;
        if (r.Tool is not null && r.Tool.Status == ToolStatusEnum.Maintenance)
        {
            r.Tool.Status = ToolStatusEnum.Available;
            r.Tool.LastServiceAt = DateOnly.FromDateTime(DateTime.Today);
            r.Tool.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    private static readonly HashSet<string> InvoiceExts = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

    public async Task<string?> SaveInvoiceAsync(IFormFile file)
    {
        if (file.Length == 0 || file.Length > 10 * 1024 * 1024
            || !InvoiceExts.Contains(Path.GetExtension(file.FileName)))
            return null;
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "invoices");
        Directory.CreateDirectory(dir);
        var fname = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using (var fs = File.Create(Path.Combine(dir, fname)))
            await file.CopyToAsync(fs);
        return $"/uploads/invoices/{fname}";
    }

    public async Task<List<Disposal>> GetDisposalsAsync() =>
        await _db.Disposals
            .Include(d => d.Tool).Include(d => d.ApprovedByUser)
            .OrderByDescending(d => d.CreatedAt).ToListAsync();

    public async Task CreateDisposalAsync(int toolId, DisposalReasonEnum reason, string? reasonDescription, string? recycleCenter, bool cancelFutureBookings)
    {
        _db.Disposals.Add(new Disposal
        {
            ToolId = toolId,
            Reason = reason,
            ReasonDescription = reasonDescription,
            RecycleCenter = recycleCenter,
            CancelFutureBookings = cancelFutureBookings,
            Status = DisposalStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task ApproveDisposalAsync(int id, int approverUserId)
    {
        var d = await _db.Disposals.Include(x => x.Tool).FirstOrDefaultAsync(x => x.DisposalId == id);
        if (d is null || d.Status != DisposalStatusEnum.Pending) return;
        d.Status = DisposalStatusEnum.PendingPickup;
        d.DisposedAt = DateOnly.FromDateTime(DateTime.Today);
        d.ApprovedByUserId = approverUserId;
        if (d.Tool is not null)
        {
            d.Tool.Status = ToolStatusEnum.Unfit;
            d.Tool.IsActive = false;
            d.Tool.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<TechnicianProfileView> GetTechnicianProfileAsync(int userId)
    {
        var myRecords = await _db.MaintenanceRecords
            .Where(m => m.TechnicianUserId == userId && m.Status == MaintenanceStatusEnum.Completed)
            .ToListAsync();
        return new TechnicianProfileView
        {
            User = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId),
            Profile = await _db.TechnicianProfiles.FirstOrDefaultAsync(p => p.UserId == userId),
            TotalRepairs = myRecords.Count,
            AvgHours = myRecords.Where(m => m.DurationHours.HasValue)
                .Select(m => (double)m.DurationHours!.Value).DefaultIfEmpty(0).Average(),
            OpenRepairs = await _db.ExternalRepairs.CountAsync(r =>
                r.Status != ExternalRepairStatusEnum.Completed && r.Status != ExternalRepairStatusEnum.Cancelled),
            TotalToolsMaintained = await _db.Tools.CountAsync(t => t.LastServiceAt != null),
        };
    }
}
