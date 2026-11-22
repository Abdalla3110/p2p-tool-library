using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    public NotificationService(AppDbContext db) => _db = db;

    public async Task NotifyAsync(int userId, string type, string title, string message,
        NotificationPriorityEnum priority = NotificationPriorityEnum.Normal, string? actionUrl = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId, Type = type, Title = title, Message = message,
            Priority = priority, ActionUrl = actionUrl, CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetForUserAsync(int userId, int take = 50) =>
        await _db.Notifications.Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt).Take(take).ToListAsync();

    public async Task<int> GetUnreadCountAsync(int userId) =>
        await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAllReadAsync(int userId)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync();
    }
}
