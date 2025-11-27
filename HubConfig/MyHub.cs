using LogLog.Service.Configurations;
using LogLog.Service.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var userId = user?.FindFirst("sub")?.Value;
            var username = user?.FindFirst("preferred_username")?.Value;
            var email = user?.FindFirst("email")?.Value;
            var givenName = user?.FindFirst("given_name")?.Value;
            var familyName = user?.FindFirst("family_name")?.Value;
            var fullname = user?.FindFirst("name")?.Value ?? $"{givenName} {familyName}".Trim();

            if (!string.IsNullOrEmpty(userId))
            {
                // Kiểm tra xem user đã có connection nào khác chưa
                var existingConnections = await _db.Connections
                    .Find(c => c.UserId == userId)
                    .ToListAsync();

                var connection = new Connection
                {
                    UserId = userId!,
                    UserFullname = fullname,
                    SignalrId = Context.ConnectionId,
                    CreatedAt = DateTime.UtcNow
                };
                await _db.Connections.InsertOneAsync(connection);

                // Chỉ thông báo "userOn" nếu đây là connection đầu tiên của user
                if (existingConnections.Count == 0)
                {
                    await Clients.Others.SendAsync("userOn", connection);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var user = Context.User;
                var userId = user?.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Xóa connection hiện tại
                    await _db.Connections.DeleteOneAsync(c => c.SignalrId == Context.ConnectionId);

                    // Kiểm tra xem user còn connection nào khác không
                    var remainingConnections = await _db.Connections
                        .Find(c => c.UserId == userId)
                        .ToListAsync();

                    // Chỉ thông báo "userOff" nếu không còn connection nào
                    if (remainingConnections.Count == 0)
                    {
                        await Clients.Others.SendAsync("userOff", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log exception
            }

            await base.OnDisconnectedAsync(exception);
        }

        //public async Task LogOut(string userId)
        //{
        //    try
        //    {
        //        await _db.Connections.DeleteManyAsync(c => c.UserId == userId);
        //        await Clients.Caller.SendAsync("logoutResponse");
        //        await Clients.Others.SendAsync("userOff", userId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        public async Task GetOnlineUsers()
        {
            try
            {
                var user = Context.User;
                var userId = user?.FindFirst("sub")?.Value;

                var onlineConnections = await _db.Connections
                    .Find(c => c.UserId != userId)
                    .ToListAsync();

                await Clients.Caller.SendAsync("getOnlineUsersResponse", onlineConnections);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task SendMsg(Message msgInfo)
        {
            await Clients.Client(msgInfo.ToConnectionId).SendAsync("sendMsgResponse", msgInfo);
        }
    }
}
