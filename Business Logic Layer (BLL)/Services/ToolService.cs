using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using DAL.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ToolService : IToolService
{
    private readonly AppDbContext _db;
    public ToolService(AppDbContext db) => _db = db;

    public async Task<Tool?> GetToolForBrowseAsync(int toolId) =>
        await _db.Tools.Include(t => t.Category).Include(t => t.Zone).Include(t => t.OwnerUser)
            .FirstOrDefaultAsync(t => t.ToolId == toolId && t.IsActive);

    public async Task<Tool?> GetAnyToolWithCategoryAsync(int toolId) =>
        await _db.Tools.Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.ToolId == toolId);

    public async Task<int> GetOwnerUserIdAsync(int toolId) =>
        await _db.Tools.Where(t => t.ToolId == toolId).Select(t => t.OwnerUserId).FirstOrDefaultAsync();

    public async Task<List<Tool>> GetFeaturedAsync(int take = 8) =>
        await _db.Tools
            .Include(t => t.Category).Include(t => t.Zone).Include(t => t.OwnerUser)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.RatingAverage)
            .ThenByDescending(t => t.TotalRentals)
            .Take(take)
            .ToListAsync();

    public async Task<List<Tool>> GetPublicToolsByOwnerAsync(int ownerId) =>
        await _db.Tools
            .Include(t => t.Category).Include(t => t.Zone)
            .Where(t => t.OwnerUserId == ownerId && t.IsActive)
            .OrderByDescending(t => t.RatingAverage)
            .ToListAsync();

    public async Task<IQueryable<Tool>> GetBrowseQueryAsync(BrowseToolsViewModel f)
    {
        var query = _db.Tools
            .Include(t => t.Category).Include(t => t.Zone).Include(t => t.OwnerUser)
            .Where(t => t.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = f.Search.Trim();
            query = query.Where(t => t.Name.Contains(s) || (t.Brand != null && t.Brand.Contains(s)));
        }
        if (f.CategoryId.HasValue) query = query.Where(t => t.CategoryId == f.CategoryId);
        if (f.ZoneId.HasValue) query = query.Where(t => t.ZoneId == f.ZoneId);
        if (f.MinPrice.HasValue) query = query.Where(t => t.PricePerDay >= f.MinPrice);
        if (f.MaxPrice.HasValue) query = query.Where(t => t.PricePerDay <= f.MaxPrice);

        if (f.AvailableFrom.HasValue && f.AvailableTo.HasValue && f.AvailableTo > f.AvailableFrom)
        {
            var from = f.AvailableFrom.Value;
            var to = f.AvailableTo.Value;
            var busyToolIds = _db.ToolAvailabilities
                .Where(a => a.Date >= from && a.Date < to && a.Status != AvailabilityStatusEnum.Available)
                .Select(a => a.ToolId).Distinct();
            query = query.Where(t => !busyToolIds.Contains(t.ToolId));
        }

        query = f.Sort switch
        {
            "priceAsc" => query.OrderBy(t => t.PricePerDay),
            "priceDesc" => query.OrderByDescending(t => t.PricePerDay),
            "newest" => query.OrderByDescending(t => t.CreatedAt),
            "nearest" => query.OrderBy(t => t.ZoneId),
            _ => query.OrderByDescending(t => t.RatingAverage).ThenByDescending(t => t.TotalRentals),
        };
        return query;
    }

    public async Task<ToolDetailsViewModel?> GetDetailsViewModelAsync(int toolId, bool isOwner)
    {
        var tool = await _db.Tools
            .Include(t => t.Category).Include(t => t.Zone).Include(t => t.OwnerUser)
            .FirstOrDefaultAsync(t => t.ToolId == toolId && t.IsActive);
        if (tool is null) return null;

        var vm = new ToolDetailsViewModel
        {
            Card = ToolCardViewModel.FromTool(tool),
            Condition = tool.Condition.ToString(),
            InsurancePercentage = tool.InsurancePercentage,
            MaxBorrowDays = tool.MaxBorrowDays,
            CleaningBufferHours = tool.CleaningBufferHours,
            BatteryHealthPercentage = tool.BatteryHealthPercentage,
            WarrantyExpiryDate = tool.WarrantyExpiryDate,
            SafetyCertificateExpiry = tool.SafetyCertificateExpiry,
            SafetyCertificateUrl = tool.SafetyCertificateUrl,
            UserManualUrl = tool.UserManualUrl,
            SafetyVideoUrl = tool.SafetyVideoUrl,
            LastServiceAt = tool.LastServiceAt,
            TotalRentals = tool.TotalRentals,
            UsageHours = tool.UsageHours,
            OwnerTier = (tool.OwnerUser?.Tier ?? MembershipTierEnum.Member).ToString(),
            OwnerTrustScore = tool.OwnerUser?.TrustScore ?? 0,
            OwnerUserId = tool.OwnerUserId,
            RecentMaintenance = await _db.MaintenanceRecords
                .Where(m => m.ToolId == toolId)
                .OrderByDescending(m => m.CreatedAt).Take(5).ToListAsync(),
            UpcomingAvailability = await _db.ToolAvailabilities
                .Where(a => a.ToolId == toolId && a.Date >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(a => a.Date).Take(60).ToListAsync(),
            Photos = await _db.ToolPhotos.Where(p => p.ToolId == toolId).OrderBy(p => p.SortOrder).ToListAsync(),
            IsOwner = isOwner,
        };
        return vm;
    }

    public async Task<Dictionary<DateOnly, string>> GetMonthAvailabilityAsync(int toolId, int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        var rows = await _db.ToolAvailabilities
            .Where(a => a.ToolId == toolId && a.Date >= first && a.Date < first.AddMonths(1))
            .ToListAsync();
        return rows.ToDictionary(a => a.Date, a => a.Status.ToString());
    }

    public async Task<List<Tool>> GetMyToolsAsync(int ownerId) =>
        await _db.Tools.Include(t => t.Category).Include(t => t.Zone)
            .Where(t => t.OwnerUserId == ownerId)
            .OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<Tool?> GetOwnedToolAsync(int toolId, int ownerId) =>
        await _db.Tools.FirstOrDefaultAsync(t => t.ToolId == toolId && t.OwnerUserId == ownerId);

    public async Task<Tool> SaveToolAsync(ToolFormViewModel m, int ownerId)
    {
        Tool tool;
        if (m.ToolId == 0)
        {
            tool = new Tool
            {
                OwnerUserId = ownerId,
                Status = ToolStatusEnum.PendingApproval,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            ApplyForm(tool, m);
            _db.Tools.Add(tool);
        }
        else
        {
            tool = await _db.Tools.FirstOrDefaultAsync(t => t.ToolId == m.ToolId && t.OwnerUserId == ownerId)
                ?? throw new InvalidOperationException("Tool not found.");
            ApplyForm(tool, m);
            tool.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return tool;
    }

    // library approval
    public async Task<List<Tool>> GetToolsForApprovalAsync(string? status)
    {
        var q = _db.Tools.Include(t => t.OwnerUser).Include(t => t.Category).Include(t => t.Zone).AsQueryable();
        if (string.IsNullOrEmpty(status))
            q = q.Where(t => t.Status == ToolStatusEnum.PendingApproval);
        else if (status != "All" && Enum.TryParse<ToolStatusEnum>(status, out var toolStatus))
            q = q.Where(t => t.Status == toolStatus);
        return await q.OrderByDescending(t => t.CreatedAt).Take(200).ToListAsync();
    }

    public async Task ApproveToolAsync(int toolId)
    {
        var t = await _db.Tools.FindAsync(toolId);
        if (t is null || t.Status != ToolStatusEnum.PendingApproval) return;
        t.Status = ToolStatusEnum.Available;
        t.UpdatedAt = DateTime.UtcNow;
        _db.Notifications.Add(new Notification
        {
            UserId = t.OwnerUserId, Type = "System", Title = "Tool approved",
            Message = $"Your tool \"{t.Name}\" is now live and bookable.",
            Priority = NotificationPriorityEnum.Normal, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task RejectToolAsync(int toolId)
    {
        var t = await _db.Tools.FindAsync(toolId);
        if (t is null || t.Status != ToolStatusEnum.PendingApproval) return;
        t.Status = ToolStatusEnum.Locked;
        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static void ApplyForm(Tool t, ToolFormViewModel m)
    {
        t.Name = m.Name.Trim();
        t.Brand = m.Brand;
        t.Description = m.Description;
        t.CategoryId = m.CategoryId;
        t.ZoneId = m.ZoneId;
        t.Condition = m.Condition;
        t.PricePerHour = m.PricePerHour;
        t.PricePerDay = m.PricePerDay;
        t.PricePerWeek = m.PricePerWeek;
        t.DepositAmount = m.DepositAmount;
        t.InsurancePercentage = m.InsurancePercentage;
        t.MaxBorrowDays = m.MaxBorrowDays;
        t.CleaningBufferHours = m.CleaningBufferHours;
        t.BatteryHealthPercentage = m.BatteryHealthPercentage;
        t.WarrantyExpiryDate = m.WarrantyExpiryDate;
        t.IsHighValue = m.IsHighValue;
    }
}
