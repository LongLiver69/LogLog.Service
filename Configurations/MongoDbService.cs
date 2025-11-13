using LogLog.Service.Domain.Entities;
using LogLog.Service.Domain.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LogLog.Service.Configurations
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Connection> Connections => _database.GetCollection<Connection>("connections");
        public IMongoCollection<Message> Messages => _database.GetCollection<Message>("messages");
    }
}
