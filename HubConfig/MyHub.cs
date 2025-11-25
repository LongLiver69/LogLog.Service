using LogLog.Service.Configurations;
using LogLog.Service.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Security.Claims;

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

            Console.WriteLine($"[Hub] User authenticated: {user?.Identity?.IsAuthenticated}");
            Console.WriteLine($"[Hub] User identity name: {user?.Identity?.Name}");

            // Try both short names (if DefaultInboundClaimTypeMap.Clear() works) 
            // and full ClaimTypes URLs (if it doesn't)
            var userId = user?.FindFirst("sub")?.Value
                      ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var username = user?.FindFirst("preferred_username")?.Value
                        ?? user?.Identity?.Name;

            var email = user?.FindFirst("email")?.Value
                     ?? user?.FindFirst(ClaimTypes.Email)?.Value;

            var givenName = user?.FindFirst("given_name")?.Value
                         ?? user?.FindFirst(ClaimTypes.GivenName)?.Value;

            var familyName = user?.FindFirst("family_name")?.Value
                          ?? user?.FindFirst(ClaimTypes.Surname)?.Value;

            var fullname = user?.FindFirst("name")?.Value
                        ?? user?.FindFirst(ClaimTypes.Name)?.Value
                        ?? $"{givenName} {familyName}".Trim();

            var connection = new Connection
            {
                UserId = userId!,
                UserFullname = fullname,
                SignalrId = currSignalrID,
                CreatedAt = DateTime.UtcNow
            };
            await _db.Connections.InsertOneAsync(connection);

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
                Console.WriteLine(ex);
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
                Console.WriteLine(ex);
            }
        }

        public async Task SendMsg(Message msgInfo)
        {
            await Clients.Client(msgInfo.ToConnectionId).SendAsync("sendMsgResponse", msgInfo);
        }
    }
}
