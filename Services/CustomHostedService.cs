using LogLog.Service.Configurations;
using MongoDB.Driver;

namespace LogLog.Service.Services
{
    public class CustomHostedService : IHostedService
    {
        private readonly MongoDbService _db;
        private readonly ILogger<CustomHostedService> _logger;

        public CustomHostedService(
            MongoDbService db,
            ILogger<CustomHostedService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Server start...");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _db.Connections.DeleteManyAsync(c => true, cancellationToken: cancellationToken);
                _logger.LogInformation("Cleaned all connections on shutdown...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing connections during shutdown");
            }

            return;
        }
    }
}