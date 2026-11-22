using DAL.Models.Entities;
using DAL.Models.ViewModels;

namespace BLL.Interfaces;

public class AdminDashboard
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalTools { get; set; }
    public int AvailableTools { get; set; }
    public int TotalBookings { get; set; }
    public decimal Gmv { get; set; }
    public decimal PlatformRevenue { get; set; }
    public int OpenDisputes { get; set; }
    public int PendingTools { get; set; }
    public int PendingKyc { get; set; }
    public List<(string Label, decimal Revenue, int Bookings)> Monthly { get; set; } = new();
    public List<AuditLog> RecentActivity { get; set; } = new();
    public List<Transaction> RecentTransactions { get; set; } = new();
}

public class AdminUsersPage
{
    public List<User> Users { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
}

public class AdminRevenuePage
{
    public List<RevenueReport> Reports { get; set; } = new();
    public List<(string Label, decimal Gmv, decimal Fees, int Bookings)> Monthly { get; set; } = new();
}

public class AdminAuditPage
{
    public List<AuditLog> Logs { get; set; } = new();
    public List<User> Users { get; set; } = new();
}

public class AdminActivityFeed
{
    public List<Transaction> Transactions { get; set; } = new();
    public List<AuditLog> AuditLogs { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
}

public class PromotionSaveResult
{
    public bool Success { get; set; }
    public string? Error { get; set; } // "NOT_FOUND" | "CODE_EXISTS"
}

public interface IAdminService
{
    Task<AdminDashboard> GetDashboardAsync();

    // users
    Task<AdminUsersPage> GetUsersPageAsync(string? role, string? search);
    Task ChangeRoleAsync(int userId, int roleId, int actorUserId, string? ip);
    Task ToggleUserActiveAsync(int userId, int actorUserId, string? ip);

    // roles
    Task<(List<Role> Roles, Dictionary<int, int> Counts)> GetRolesPageAsync();

    // add user
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateUserAsync(AdminUserFormViewModel m);

    // settings
    Task<PlatformSetting> GetSettingsAsync();
    Task SaveSettingsAsync(PlatformSetting m, int actorUserId);

    // audit log page
    Task<AdminAuditPage> GetAuditLogPageAsync(string? action, int? userId);

    // revenue
    Task<AdminRevenuePage> GetRevenuePageAsync();
    Task GenerateRevenueReportAsync(int actorUserId);

    // promotions
    Task<List<Promotion>> GetPromotionsAsync();
    Task<Promotion?> GetPromotionForEditAsync(int promotionId);
    Task<PromotionSaveResult> SavePromotionAsync(PromotionFormViewModel m, int actorUserId);
    Task TogglePromotionAsync(int promotionId, int actorUserId);

    // bookings / transactions / tools
    Task<List<Booking>> GetAllBookingsAsync(string? status);
    Task<List<Transaction>> GetTransactionsAsync(string? type);
    Task<List<Tool>> GetAllToolsAsync();
    Task SuspendToolAsync(int toolId, int actorUserId);
    Task ReinstateToolAsync(int toolId, int actorUserId);

    // activity
    Task<AdminActivityFeed> GetActivityFeedAsync();
}
