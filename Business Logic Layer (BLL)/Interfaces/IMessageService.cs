using DAL.Models.Entities;

namespace BLL.Interfaces;

public interface IMessageService
{
    Task<List<Conversation>> GetConversationsAsync(int userId);
    Task<Conversation?> StartConversationAsync(int toolId, int userId);
    Task<(Conversation? Conversation, List<Message> Messages)> GetConversationAsync(Guid conversationId, int userId);
    Task MarkReadAsync(Guid conversationId, int userId);
    Task SendMessageAsync(Guid conversationId, int senderId, string senderName, string body);
    Task<bool> ToolExistsAsync(int toolId);
    Task<bool> ConversationExistsAsync(Guid conversationId);
    Task<bool> IsParticipantAsync(Guid conversationId, int userId);
}
