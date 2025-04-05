using Microsoft.AspNetCore.SignalR;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo.Hubs
{
    public class UserHub : Hub
    {
        // Called when a new connection is established with the hub
        public override async Task OnConnectedAsync()
        {
            // Notify the specific client that just connected
            await Clients.Caller.SendAsync("ReceiveUserStatusChange", "", "connected");
            await base.OnConnectedAsync();
        }

        // Send updated user list to all connected clients
        public async Task SendUserListUpdate(IEnumerable<User> users)
        {
            await Clients.All.SendAsync("ReceiveUserListUpdate", users);
        }

        // Send notification about user status change
        public async Task SendUserStatusChange(string username, string action)
        {
            await Clients.All.SendAsync("ReceiveUserStatusChange", username, action);
        }
    }
} 