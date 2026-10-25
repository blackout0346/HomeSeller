using System.Text.Json;
using System.Threading.RateLimiting;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using HomeSellerServer.Service;
using HomeSellerServer.StoreService;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddTransient<IHashPassword, HashPassword>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IUserStore, UserStore>();

builder.Services.AddTransient<IRabbitMqProducer, RabbitMqProducer>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

   
}
app.MapGet("/", () => "Hello World!");

app.Run();
