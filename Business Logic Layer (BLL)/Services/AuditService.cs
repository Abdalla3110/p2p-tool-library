using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    public AuditService(AppDbContext db) => _db = db;

    public async Task LogAsync(int? userId, string action, string entityType, int? entityId, string? details)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId, Action = action, EntityType = entityType,
            EntityId = entityId?.ToString(), Details = details,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLogRow>> GetLogsAsync(int take = 100) =>
        await _db.AuditLogs.OrderByDescending(a => a.CreatedAt).Take(take)
            .Select(a => new AuditLogRow
            {
                CreatedAt = a.CreatedAt, UserId = a.UserId, Action = a.Action,
                EntityType = a.EntityType, Details = a.Details,
            }).ToListAsync();
}
