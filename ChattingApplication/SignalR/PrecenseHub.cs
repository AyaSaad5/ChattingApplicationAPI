using ChattingApplication.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChattingApplication.SignalR
{
    [Authorize]
    public class PrecenseHub : Hub
    {
        private readonly PrecenseTracker tracker;

        public PrecenseHub(PrecenseTracker tracker)
        {
            this.tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await this.tracker.UserConnected(Context.User.GetUserName(), Context.ConnectionId);

            if(isOnline)
                 await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUserName());

            var currentUser = await this.tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUser);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var isOffline = await this.tracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);
            if(isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());

            await base.OnDisconnectedAsync(exception);
        }
    }
}
