using LogLog.Service;
using LogLog.Service.Configurations;
using LogLog.Service.Domain.Models;
using LogLog.Service.HubConfig;

var builder = WebApplication.CreateBuilder(args);

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
            .AllowCredentials(); // quan trong voi SignalR
    });
});
//builder.Services.AddConnectionString(builder.Configuration);

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbService>();

builder.Services.AddAuthentication("Bearer")
   .AddJwtBearer(options =>
   {
       options.Authority = "http://localhost:8080/realms/master";
       options.RequireHttpsMetadata = false;
       options.Audience = "loglog-client";
   });

var app = builder.Build();

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAngular");

app.MapHub<MyHub>("/hub");

app.MapControllers();

app.Run();
