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
            var fullname = user?.FindFirst("name")?.Value;

            //var currconnect = new Connection
            //{
            //    UserId = User.Id,
            //    SignalrId = currSignalrID,
            //    Timestamp = DateTime.Now
            //};
            //await _db.Connections.AddAsync(currconnect);
            //await _db.SaveChangesAsync();

            //var res = new UserDto()
            //{
            //    UserId = tempUser.Id,
            //    FullName = tempUser.Name,
            //    SignalrId = currSignalrID
            //};

            //await Clients.Caller.SendAsync("authMeResponseSuccess", res);
            //await Clients.Others.SendAsync("userOn", res);
            Console.WriteLine($"User connected: {user} ({userId}) with SignalR ID: {currSignalrID}");
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
