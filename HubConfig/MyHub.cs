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
            string currSignalrID = Context.ConnectionId;

            var user = Context.User;
            var userId = user?.FindFirst("sub")?.Value;
            var username = user?.FindFirst("preferred_username")?.Value;
            var email = user?.FindFirst("email")?.Value;
            var givenName = user?.FindFirst("given_name")?.Value;
            var familyName = user?.FindFirst("family_name")?.Value;
            var fullname = user?.FindFirst("name")?.Value ?? $"{givenName} {familyName}".Trim();

            var connection = new Connection
            {
                UserId = userId!,
                UserFullname = fullname,
                SignalrId = currSignalrID,
                CreatedAt = DateTime.UtcNow
            };
            await _db.Connections.InsertOneAsync(connection);
            await Clients.Others.SendAsync("userOn", connection);

            await base.OnConnectedAsync();
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
                var connection = await _db.Connections
                    .Find(c => c.SignalrId == Context.ConnectionId)
                    .FirstOrDefaultAsync();

                var currUserId = connection?.UserId;

                var onlineConnections = await _db.Connections
                    .Find(c => c.UserId != currUserId)
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
