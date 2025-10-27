using Microsoft.AspNetCore.SignalR;
using LogLog.Service.Domain.Models;
using LogLog.Service.Domain;

namespace LogLog.Service.HubConfig
{
    public class MyHub : Hub
    {
        private readonly DatabaseContext _context;

        public MyHub(DatabaseContext context)
        {
            _context = context;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            int currUserId = _context.Connections.Where(c => c.SignalrId == Context.ConnectionId).Select(c => c.UserId).SingleOrDefault();
            _context.Connections.RemoveRange(_context.Connections.Where(p => p.UserId == currUserId).ToList());
            _context.SaveChanges();
            Clients.Others.SendAsync("userOff", currUserId);

            return base.OnDisconnectedAsync(exception);
        }

        public void LogOut(int personId)
        {
            _context.Connections.RemoveRange(_context.Connections.Where(p => p.UserId == personId).ToList());
            _context.SaveChanges();
            Clients.Caller.SendAsync("logoutResponse");
            Clients.Others.SendAsync("userOff", personId);
        }

        public async Task GetOnlineUsers()
        {
            int currUserId = _context.Connections.Where(c => c.SignalrId == Context.ConnectionId).Select(c => c.UserId).SingleOrDefault();
            var onlineUsers = _context.Connections
                .Where(c => c.UserId != currUserId)
                .Select(c =>
                    new UserDto()
                    {
                        UserId = c.UserId,
                        FullName = _context.Users.Where(p => p.Id == c.UserId).Select(p => p.Name).SingleOrDefault(),
                        SignalrId = c.SignalrId
                    }
                ).ToList();
            await Clients.Caller.SendAsync("getOnlineUsersResponse", onlineUsers);
        }

        public async Task SendMsg(Message msgInfo)
        {
            await Clients.Client(msgInfo.ToConnectionId).SendAsync("sendMsgResponse", msgInfo);
        }
    }
}
