namespace BLL.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string entityType, int? entityId, string? details);
    Task<List<AuditLogRow>> GetLogsAsync(int take = 100);
}

public class AuditLogRow
{
    public DateTime CreatedAt { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string? Details { get; set; }
}
