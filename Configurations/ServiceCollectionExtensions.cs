using LogLog.Service.Domain;
using LogLog.Service.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LogLog.Service
{
    public static class ServiceCollectionExtensions
    {
        //public static IServiceCollection AddConnectionString(this IServiceCollection services, IConfiguration configuration)
        //{
        //    //// get default connection
        //    var strDDConnect = configuration.GetConnectionString("DefaultConnection");

        //    services.AddMvc();
        //    services.AddEntityFrameworkNpgsql().AddDbContext<DatabaseContext>(opt =>
        //        opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        //    return services;
        //}

        public static IServiceCollection AddConnectionString(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            // Register MongoClient
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            // Register Database
            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });

            return services;
        }
    }
}