using Microsoft.AspNetCore.SignalR;

namespace BasicWebNovelAPI.Hubs;

public class CommentHub : Hub
{
    public async Task SendComment(string message)
    {
        await Clients.All.SendAsync("ReceiveComment", message);
    }
}