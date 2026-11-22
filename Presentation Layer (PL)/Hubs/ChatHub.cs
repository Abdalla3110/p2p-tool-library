using Microsoft.AspNetCore.SignalR;

namespace PL.Hubs;

public class ChatHub : Hub
{
    public async Task JoinConversation(string conversationId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"conv-{conversationId}");

    public async Task LeaveConversation(string conversationId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv-{conversationId}");
}
