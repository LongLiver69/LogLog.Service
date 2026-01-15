using LogLog.Service.Configurations;
using LogLog.Service.Domain.Models;
using LogLog.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Minio;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

//builder.Logging.ClearProviders();  // Tat cac logger mac dinh
//
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
//    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Fatal)
//    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Fatal)
//    .MinimumLevel.Override("System", LogEventLevel.Fatal)
//    .MinimumLevel.Information()
//    .ReadFrom.Configuration(builder.Configuration)
//    .WriteTo.Console()
//    .WriteTo.PostgreSQL(
//        connectionString: builder.Configuration.GetConnectionString("PostgreSQL")!,
//        tableName: "logs",
//        needAutoCreateTable: true,
//        columnOptions: new Dictionary<string, ColumnWriterBase>
//        {
//            { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
//            { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
//            { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
//            { "username", new SinglePropertyColumnWriter("Username", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
//            { "timestamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
//            { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) }
//        })
//    .Enrich.FromLogContext()
//    .CreateLogger();

//builder.Host.UseSerilog();

// Add services to the container.  
builder.Services.AddSignalR(option =>
{
    option.EnableDetailedErrors = true;
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MinioClient()
        .WithEndpoint(config["MinIO:Endpoint"])
        .WithCredentials(
            config["MinIO:AccessKey"],
            config["MinIO:SecretKey"])
        .WithSSL(false)
        .Build();
});

//builder.Services.AddConnectionString(builder.Configuration);
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbService>();

builder.Services.AddHttpClient();

var hubPattern = builder.Configuration.GetValue<string>("HubPattern");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.Authority = builder.Configuration["Keycloak:Authority"];
       options.Audience = builder.Configuration["Keycloak:Audience"];
       options.RequireHttpsMetadata = false;
       options.MapInboundClaims = false;  // This is the KEY setting to keep original claim names!

       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateAudience = false,  // Keycloak token has multiple audiences
           ValidateIssuer = true,
           ValidateLifetime = true,
           NameClaimType = "preferred_username",
           RoleClaimType = "realm_access.roles",
       };

       options.Events = new JwtBearerEvents
       {
           OnMessageReceived = context =>
           {
               var accessToken = context.Request.Query["access_token"];
               var path = context.Request.Path;

               if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(hubPattern))
               {
                   context.Token = accessToken;
               }
               return Task.CompletedTask;
           },
           //OnTokenValidated = context =>
           //{
           //    Console.WriteLine("[JWT] Token validated.");
           //    return Task.CompletedTask;
           //},
           //OnAuthenticationFailed = context =>
           //{
           //    Console.WriteLine($"[JWT] Auth failed: {context.Exception.Message}");
           //    return Task.CompletedTask;
           //}
       };
   });

builder.Services.AddAuthorization();
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

// Delete all signalr connections on server stop
builder.Services.AddHostedService<CustomHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SignalrHub>(hubPattern!);

app.MapControllers();

app.Run();
