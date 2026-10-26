using System.Threading.RateLimiting;
using HomeSeller.CartService.Interfaces;
using HomeSeller.CartService.Module;
using HomeSeller.CartService.Services;
using HomeSeller.CartService.StoreServices;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

var redisHost = builder.Configuration["Redis:Host"] ?? "localhost";
var redisPort = builder.Configuration["Redis:Port"] ?? "6379";
var redispassword = builder.Configuration["Redis:Password"];
var configOptions = new ConfigurationOptions
{
    EndPoints = { $"{redisHost}:{redisPort}" },
    Password = string.IsNullOrEmpty(redispassword) ? "guest" : redispassword,
    AbortOnConnectFail = false,
};
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
builder.Services.AddTransient<ICartStore, CartStore>();
builder.Services.AddTransient<ICartService, CartService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configOptions));
builder.Services.AddDbContext<CartDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbcontext = scope.ServiceProvider.GetRequiredService<CartDbContext>();
    await dbcontext.Database.MigrateAsync();
}

app.MapGet("/", () => "Hello World!");

app.Run();