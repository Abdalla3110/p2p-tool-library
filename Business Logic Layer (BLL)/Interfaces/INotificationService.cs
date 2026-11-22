using DAL.Models.Entities;

namespace BLL.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(int userId, string type, string title, string message, NotificationPriorityEnum priority = NotificationPriorityEnum.Normal, string? actionUrl = null);
    Task<List<Notification>> GetForUserAsync(int userId, int take = 50);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAllReadAsync(int userId);
}
