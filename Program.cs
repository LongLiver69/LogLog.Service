using LogLog.Service.Configurations;
using LogLog.Service.Domain.Models;
using LogLog.Service.HubConfig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

var hubPattern = builder.Configuration.GetValue<string>("HubPattern");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.Authority = "http://localhost:8080/realms/master";
       options.Audience = "loglog-client";
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

// Delete all connections on server stop
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

app.MapHub<MyHub>(hubPattern ?? "/hub");

app.MapControllers();

app.Run();
