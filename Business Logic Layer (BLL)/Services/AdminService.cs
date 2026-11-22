using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using DAL.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    public AdminService(AppDbContext db) => _db = db;

    private async Task LogAsync(int? userId, string action, string entityType, string? entityId, string? details, string? ip)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId, Action = action, EntityType = entityType,
            EntityId = entityId, Details = details, IpAddress = ip,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    /* ---------------- Dashboard ---------------- */

    public async Task<AdminDashboard> GetDashboardAsync()
    {
        var openStatuses = new[] { DisputeStatusEnum.Open, DisputeStatusEnum.InReview, DisputeStatusEnum.Reopened };
        var dash = new AdminDashboard
        {
            TotalUsers = await _db.Users.CountAsync(),
            ActiveUsers = await _db.Users.CountAsync(u => u.IsActive),
            TotalTools = await _db.Tools.CountAsync(),
            AvailableTools = await _db.Tools.CountAsync(t => t.Status == ToolStatusEnum.Available && t.IsActive),
            TotalBookings = await _db.Bookings.CountAsync(),
            Gmv = await _db.Bookings.Where(b => b.Status != BookingStatusEnum.Cancelled)
                .SumAsync(b => (decimal?)b.RentalAmount) ?? 0,
            PlatformRevenue = await _db.Bookings.Where(b => b.Status != BookingStatusEnum.Cancelled)
                .SumAsync(b => (decimal?)b.PlatformFee) ?? 0,
            OpenDisputes = await _db.Disputes.CountAsync(d => openStatuses.Contains(d.Status)),
            PendingTools = await _db.Tools.CountAsync(t => t.Status == ToolStatusEnum.PendingApproval),
            PendingKyc = await _db.Users.CountAsync(u => u.KycStatus == KycStatusEnum.Pending),
            RecentActivity = await _db.AuditLogs.Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt).Take(10).ToListAsync(),
            RecentTransactions = await _db.Transactions.Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt).Take(8).ToListAsync(),
        };

        for (var i = 5; i >= 0; i--)
        {
            var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
            var end = start.AddMonths(1);
            var rev = await _db.Bookings
                .Where(b => b.Status == BookingStatusEnum.Completed && b.UpdatedAt >= start && b.UpdatedAt < end)
                .SumAsync(b => (decimal?)b.PlatformFee) ?? 0;
            var cnt = await _db.Bookings.CountAsync(b => b.Status == BookingStatusEnum.Completed && b.UpdatedAt >= start && b.UpdatedAt < end);
            dash.Monthly.Add((start.ToString("MMM yy"), rev, cnt));
        }
        return dash;
    }

    /* ---------------- Users ---------------- */

    public async Task<AdminUsersPage> GetUsersPageAsync(string? role, string? search)
    {
        var q = _db.Users.Include(u => u.Role).Include(u => u.Zone).AsQueryable();
        if (!string.IsNullOrEmpty(role) && role != "All")
            q = q.Where(u => u.Role!.Name == role);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(u => u.FullName.Contains(s) || u.Email.Contains(s));
        }
        return new AdminUsersPage
        {
            Users = await q.OrderByDescending(u => u.JoinedAt).Take(300).ToListAsync(),
            Roles = await _db.Roles.OrderBy(r => r.Name).ToListAsync(),
        };
    }

    public async Task ChangeRoleAsync(int userId, int roleId, int actorUserId, string? ip)
    {
        var u = await _db.Users.FindAsync(userId);
        if (u is null || u.UserId == actorUserId) return;
        u.RoleId = roleId;
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "Update", "User", userId.ToString(), $"role -> {roleId}", ip);
    }

    public async Task ToggleUserActiveAsync(int userId, int actorUserId, string? ip)
    {
        var u = await _db.Users.FindAsync(userId);
        if (u is null || u.UserId == actorUserId) return;
        u.IsActive = !u.IsActive;
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, u.IsActive ? "Update" : "Suspension", "User", userId.ToString(), null, ip);
    }

    /* ---------------- Roles ---------------- */

    public async Task<(List<Role> Roles, Dictionary<int, int> Counts)> GetRolesPageAsync()
    {
        var roles = await _db.Roles.ToListAsync();
        var counts = await _db.Users
            .GroupBy(u => u.RoleId).Select(g => new { g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Cnt);
        return (roles, counts);
    }

    /* ---------------- Add user ---------------- */

    public async Task<bool> EmailExistsAsync(string email) =>
        await _db.Users.AnyAsync(u => u.Email == email);

    public async Task<User> CreateUserAsync(AdminUserFormViewModel m)
    {
        var user = new User
        {
            FullName = m.FullName.Trim(),
            Email = m.Email.Trim(),
            Phone = m.Phone,
            PasswordHash = PasswordHasher.Hash(m.Password),
            RoleId = m.RoleId,
            ZoneId = m.ZoneId,
            KycStatus = KycStatusEnum.Pending,
            Tier = MembershipTierEnum.Member,
            TrustScore = 50,
            RatingAverage = 0,
            ReviewCount = 0,
            ReferralCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            WalletBalance = 0,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            IsVerified = false,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    /* ---------------- Settings ---------------- */

    public async Task<PlatformSetting> GetSettingsAsync()
    {
        var s = await _db.PlatformSettings.FirstOrDefaultAsync();
        if (s is not null) return s;
        s = new PlatformSetting
        {
            PlatformCommissionPercentage = 18,
            InsurancePercentage = 5,
            DefaultDepositPercentage = 20,
            DefaultMaxBorrowDays = 7,
            DefaultCleaningBufferHours = 10,
            LateReturnGraceHours = 24,
            LateReturnPenaltyPerDay = 50,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.PlatformSettings.Add(s);
        await _db.SaveChangesAsync();
        return s;
    }

    public async Task SaveSettingsAsync(PlatformSetting m, int actorUserId)
    {
        var s = await _db.PlatformSettings.FindAsync(m.SettingsId);
        if (s is null) return;
        s.PlatformCommissionPercentage = m.PlatformCommissionPercentage;
        s.InsurancePercentage = m.InsurancePercentage;
        s.DefaultDepositPercentage = m.DefaultDepositPercentage;
        s.DefaultMaxBorrowDays = m.DefaultMaxBorrowDays;
        s.DefaultCleaningBufferHours = m.DefaultCleaningBufferHours;
        s.LateReturnGraceHours = m.LateReturnGraceHours;
        s.LateReturnPenaltyPerDay = m.LateReturnPenaltyPerDay;
        s.BookingAutoApproveEnabled = m.BookingAutoApproveEnabled;
        s.EmailBookingConfirmationsEnabled = m.EmailBookingConfirmationsEnabled;
        s.SmsLateReturnAlertsEnabled = m.SmsLateReturnAlertsEnabled;
        s.PushNotificationsEnabled = m.PushNotificationsEnabled;
        s.WeeklyMaintenanceRemindersEnabled = m.WeeklyMaintenanceRemindersEnabled;
        s.PlatformPaused = m.PlatformPaused;
        s.UpdatedAt = DateTime.UtcNow;
        s.UpdatedByUserId = actorUserId;
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "ConfigChange", "PlatformSetting", s.SettingsId.ToString(), null, null);
    }

    /* ---------------- Audit log ---------------- */

    public async Task<AdminAuditPage> GetAuditLogPageAsync(string? action, int? userId)
    {
        var q = _db.AuditLogs.Include(a => a.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(action))
            q = q.Where(a => a.Action.Contains(action));
        if (userId.HasValue)
            q = q.Where(a => a.UserId == userId);
        return new AdminAuditPage
        {
            Logs = await q.OrderByDescending(a => a.CreatedAt).Take(300).ToListAsync(),
            Users = await _db.Users.OrderBy(u => u.FullName)
                .Select(u => new User { UserId = u.UserId, FullName = u.FullName }).ToListAsync(),
        };
    }

    /* ---------------- Revenue ---------------- */

    public async Task<AdminRevenuePage> GetRevenuePageAsync()
    {
        var page = new AdminRevenuePage
        {
            Reports = await _db.RevenueReports.OrderByDescending(r => r.PeriodMonth).ToListAsync(),
        };
        for (var i = 5; i >= 0; i--)
        {
            var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
            var end = start.AddMonths(1);
            var bs = await _db.Bookings
                .Where(b => b.Status == BookingStatusEnum.Completed && b.UpdatedAt >= start && b.UpdatedAt < end)
                .ToListAsync();
            page.Monthly.Add((start.ToString("MMM yy"),
                bs.Sum(b => b.RentalAmount + b.InsuranceAmount),
                bs.Sum(b => b.PlatformFee),
                bs.Count));
        }
        return page;
    }

    public async Task GenerateRevenueReportAsync(int actorUserId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateOnly(now.Year, now.Month, 1);
        var next = monthStart.AddMonths(1);

        var bookings = await _db.Bookings
            .Where(b => b.Status == BookingStatusEnum.Completed
                && b.UpdatedAt >= monthStart.ToDateTime(TimeOnly.MinValue)
                && b.UpdatedAt < next.ToDateTime(TimeOnly.MinValue))
            .ToListAsync();

        _db.RevenueReports.Add(new RevenueReport
        {
            PeriodMonth = monthStart, Source = "PlatformFees",
            TransactionCount = bookings.Count, Amount = bookings.Sum(b => b.PlatformFee),
            CreatedAt = DateTime.UtcNow,
        });
        _db.RevenueReports.Add(new RevenueReport
        {
            PeriodMonth = monthStart, Source = "Insurance",
            TransactionCount = bookings.Count, Amount = bookings.Sum(b => b.InsuranceAmount),
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "Other", "RevenueReport", monthStart.ToString("yyyy-MM"), null, null);
    }

    /* ---------------- Promotions ---------------- */

    public async Task<List<Promotion>> GetPromotionsAsync() =>
        await _db.Promotions.Include(p => p.Category).OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<Promotion?> GetPromotionForEditAsync(int promotionId) =>
        await _db.Promotions.FindAsync(promotionId);

    public async Task<PromotionSaveResult> SavePromotionAsync(PromotionFormViewModel m, int actorUserId)
    {
        m.Code = m.Code.Trim().ToUpper();
        if (await _db.Promotions.AnyAsync(p => p.Code == m.Code && p.PromotionId != m.PromotionId))
            return new PromotionSaveResult { Error = "CODE_EXISTS" };

        if (m.PromotionId == 0)
        {
            _db.Promotions.Add(new Promotion
            {
                Code = m.Code, Description = m.Description, DiscountType = m.DiscountType,
                DiscountPercentage = m.DiscountPercentage, DiscountAmount = m.DiscountAmount,
                CategoryId = m.CategoryId, StartDate = m.StartDate, EndDate = m.EndDate,
                MaxUses = m.MaxUses, UsedCount = 0, Status = m.Status,
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
            await LogAsync(actorUserId, "Create", "Promotion", m.Code, null, null);
        }
        else
        {
            var p = await _db.Promotions.FindAsync(m.PromotionId);
            if (p is null) return new PromotionSaveResult { Error = "NOT_FOUND" };
            p.Code = m.Code; p.Description = m.Description; p.DiscountType = m.DiscountType;
            p.DiscountPercentage = m.DiscountPercentage; p.DiscountAmount = m.DiscountAmount;
            p.CategoryId = m.CategoryId; p.StartDate = m.StartDate; p.EndDate = m.EndDate;
            p.MaxUses = m.MaxUses; p.Status = m.Status;
            await _db.SaveChangesAsync();
            await LogAsync(actorUserId, "Update", "Promotion", m.Code, null, null);
        }
        return new PromotionSaveResult { Success = true };
    }

    public async Task TogglePromotionAsync(int promotionId, int actorUserId)
    {
        var p = await _db.Promotions.FindAsync(promotionId);
        if (p is null) return;
        p.Status = p.Status == PromotionStatusEnum.Active ? PromotionStatusEnum.Disabled : PromotionStatusEnum.Active;
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "Update", "Promotion", p.Code, $"status -> {p.Status}", null);
    }

    /* ---------------- Bookings / Transactions / Tools ---------------- */

    public async Task<List<Booking>> GetAllBookingsAsync(string? status)
    {
        var q = _db.Bookings.Include(b => b.Tool).Include(b => b.BorrowerUser).Include(b => b.LenderUser).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All"
            && Enum.TryParse<BookingStatusEnum>(status, out var bookingStatus))
            q = q.Where(b => b.Status == bookingStatus);
        return await q.OrderByDescending(b => b.CreatedAt).Take(300).ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsAsync(string? type)
    {
        var q = _db.Transactions.Include(t => t.User).AsQueryable();
        if (!string.IsNullOrEmpty(type) && type != "All"
            && Enum.TryParse<TransactionTypeEnum>(type, out var txnType))
            q = q.Where(t => t.Type == txnType);
        return await q.OrderByDescending(t => t.CreatedAt).Take(500).ToListAsync();
    }

    public async Task<List<Tool>> GetAllToolsAsync() =>
        await _db.Tools.Include(t => t.Category).Include(t => t.OwnerUser)
            .OrderByDescending(t => t.CreatedAt).Take(300).ToListAsync();

    public async Task SuspendToolAsync(int toolId, int actorUserId)
    {
        var t = await _db.Tools.FindAsync(toolId);
        if (t is null) return;
        t.IsActive = false;
        t.Status = ToolStatusEnum.Unfit;
        t.UpdatedAt = DateTime.UtcNow;
        _db.Notifications.Add(new Notification
        {
            UserId = t.OwnerUserId, Type = "Tool", Title = "Tool suspended",
            Message = $"Your tool \"{t.Name}\" was suspended by the administration.",
            Priority = NotificationPriorityEnum.High, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "Update", "Tool", toolId.ToString(), "suspended by admin", null);
    }

    public async Task ReinstateToolAsync(int toolId, int actorUserId)
    {
        var t = await _db.Tools.FindAsync(toolId);
        if (t is null) return;
        t.IsActive = true;
        t.Status = ToolStatusEnum.Available;
        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "Update", "Tool", toolId.ToString(), "reinstated by admin", null);
    }

    /* ---------------- Activity ---------------- */

    public async Task<AdminActivityFeed> GetActivityFeedAsync() => new()
    {
        Transactions = await _db.Transactions.Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt).Take(50).ToListAsync(),
        AuditLogs = await _db.AuditLogs.Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt).Take(50).ToListAsync(),
        Notifications = await _db.Notifications
            .OrderByDescending(n => n.CreatedAt).Take(20).ToListAsync(),
    };
}
