using System.Text;
using System.Threading.RateLimiting;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using HomeSellerServer.Service;
using HomeSellerServer.Services;
using HomeSellerServer.StoreService;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
var redisHost = builder.Configuration["Redis:Host"] ?? "localhost";
var redisPort = builder.Configuration["Redis:Port"] ?? "6379";
var redispassword = builder.Configuration["Redis:Password"];
var configOptions = new ConfigurationOptions
{
    EndPoints = { $"{redisHost}:{redisPort}" },
    Password = string.IsNullOrEmpty(redispassword) ? "guest" : redispassword,
    AbortOnConnectFail = false,
};
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var host =builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
        cfg.Host(host, "/", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            }
        );
    });
});
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configOptions));
builder.Services.AddTransient<IHashPassword, HashPassword>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IUserStore, UserStore>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("sliding_window", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.SegmentsPerWindow = 4;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],     
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero 
    };
});
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await context.Database.MigrateAsync();

}

app.MapGet("/", () => "Hello World!");

app.Run();
