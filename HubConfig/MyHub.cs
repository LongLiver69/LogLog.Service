using Microsoft.AspNetCore.SignalR;
using LogLog.Service.Domain.Entities;
using LogLog.Service.Configurations;
using MongoDB.Driver;

namespace LogLog.Service.HubConfig
{
    public class MyHub : Hub
    {
        private readonly MongoDbService _db;

        public MyHub(MongoDbService db)
        {
            _db = db;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var connection = await _db.Connections
                    .Find(c => c.SignalrId == Context.ConnectionId)
                    .FirstOrDefaultAsync();

                if (connection != null)
                {
                    var userId = connection.UserId;
                    await _db.Connections.DeleteManyAsync(c => c.UserId == userId);
                    await Clients.Others.SendAsync("userOff", userId);
                }
            }
            catch (Exception ex)
            {
                // Optionally log exception
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task LogOut(string userId)
        {
            try
            {
                await _db.Connections.DeleteManyAsync(c => c.UserId == userId);
                await Clients.Caller.SendAsync("logoutResponse");
                await Clients.Others.SendAsync("userOff", userId);
            }
            catch (Exception ex)
            {
                // Optionally log exception
            }
        }

        public async Task GetOnlineUsers()
        {
            try
            {
                var connection = await _db.Connections
                    .Find(c => c.SignalrId == Context.ConnectionId)
                    .FirstOrDefaultAsync();

                var currUserId = connection?.UserId;

                var onlineConnections = await _db.Connections
                    .Find(c => c.UserId != currUserId)
                    .ToListAsync();

                var onlineUsers = onlineConnections.Select(c => new UserDto
                {
                    UserId = c.UserId,
                    FullName = c.UserFullname,
                    SignalrId = c.SignalrId
                }).ToList();

                await Clients.Caller.SendAsync("getOnlineUsersResponse", onlineUsers);
            }
            catch (Exception ex)
            {
                // Optionally log exception
            }
        }

        public async Task SendMsg(Message msgInfo)
        {
            await Clients.Client(msgInfo.ToConnectionId).SendAsync("sendMsgResponse", msgInfo);
        }
    }
}
