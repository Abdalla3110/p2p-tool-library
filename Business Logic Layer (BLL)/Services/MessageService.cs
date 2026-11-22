using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PL.Hubs;

namespace BLL.Services;

public class MessageService : IMessageService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ChatHub> _hub;

    public MessageService(AppDbContext db, IHubContext<ChatHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<List<Conversation>> GetConversationsAsync(int userId) =>
        await _db.Conversations
            .Include(c => c.User1).Include(c => c.User2).Include(c => c.Tool)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();

    public async Task<Conversation?> StartConversationAsync(int toolId, int userId)
    {
        var tool = await _db.Tools.Include(t => t.OwnerUser).FirstOrDefaultAsync(t => t.ToolId == toolId);
        if (tool is null || tool.OwnerUserId == userId) return null;

        var existing = await _db.Conversations.FirstOrDefaultAsync(c =>
            c.ToolId == toolId &&
            ((c.User1Id == userId && c.User2Id == tool.OwnerUserId) ||
             (c.User1Id == tool.OwnerUserId && c.User2Id == userId)));
        if (existing is not null) return existing;

        var conv = new Conversation
        {
            User1Id = userId,
            User2Id = tool.OwnerUserId,
            ToolId = toolId,
            Subject = $"Question about {tool.Name}",
            CreatedAt = DateTime.UtcNow,
        };
        _db.Conversations.Add(conv);
        await _db.SaveChangesAsync();
        return conv;
    }

    public async Task<(Conversation?, List<Message>)> GetConversationAsync(Guid conversationId, int userId)
    {
        var conv = await _db.Conversations
            .Include(c => c.User1).Include(c => c.User2).Include(c => c.Tool)
            .Include(c => c.Messages.OrderBy(m => m.SentAt))
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        if (conv is null || (conv.User1Id != userId && conv.User2Id != userId))
            return (null, new List<Message>());
        return (conv, conv.Messages.ToList());
    }

    public async Task MarkReadAsync(Guid conversationId, int userId)
    {
        var conv = await _db.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        if (conv is null) return;
        var changed = false;
        foreach (var m in conv.Messages.Where(m => !m.IsRead && m.SenderUserId != userId))
        {
            m.IsRead = true;
            m.ReadAt = DateTime.UtcNow;
            changed = true;
        }
        if (changed) await _db.SaveChangesAsync();
    }

    public async Task SendMessageAsync(Guid conversationId, int senderId, string senderName, string body)
    {
        var conv = await _db.Conversations.FindAsync(conversationId);
        if (conv is null) return;

        var msg = new Message
        {
            ConversationId = conversationId,
            SenderUserId = senderId,
            Body = body.Trim(),
            SentAt = DateTime.UtcNow,
        };
        _db.Messages.Add(msg);
        conv.LastMessageAt = msg.SentAt;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group($"conv-{conversationId}").SendAsync("ReceiveMessage", new
        {
            senderId,
            senderName,
            body = msg.Body,
            sentAt = msg.SentAt.ToString("MM-dd HH:mm"),
        });
    }
    public async Task<bool> ToolExistsAsync(int toolId) =>
        await _db.Tools.AnyAsync(t => t.ToolId == toolId);

    public async Task<bool> ConversationExistsAsync(Guid conversationId) =>
        await _db.Conversations.AnyAsync(c => c.ConversationId == conversationId);

    public async Task<bool> IsParticipantAsync(Guid conversationId, int userId) =>
        await _db.Conversations.AnyAsync(c => c.ConversationId == conversationId
            && (c.User1Id == userId || c.User2Id == userId));
}
