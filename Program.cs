using LogLog.Service;
using LogLog.Service.Configurations;
using LogLog.Service.Domain.Models;
using LogLog.Service.HubConfig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// IMPORTANT: keep original claim names from Keycloak
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.Authority = "http://localhost:8080/realms/master";
       options.RequireHttpsMetadata = false;
       options.Audience = "loglog-client";  // Keycloak uses "account" as audience
       
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
               var path = context.HttpContext.Request.Path;
               if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
               {
                   context.Token = accessToken;
               }
               return Task.CompletedTask;
           },
           OnTokenValidated = context =>
           {
               foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<Claim>())
               {
                   Console.WriteLine($"  [{claim.Type}] = {claim.Value}");
               }
               return Task.CompletedTask;
           },
           //OnAuthenticationFailed = context =>
           //{
           //    Console.WriteLine($"[JWT] Auth failed: {context.Exception.Message}");
           //    return Task.CompletedTask;
           //}
       };
   });

builder.Services.AddAuthorization();

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

app.MapHub<MyHub>("/hub");

app.MapControllers();

app.Run();
