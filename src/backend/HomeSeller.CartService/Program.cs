using System.Threading.RateLimiting;
using HomeSeller.CartService.Module;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;


DotNetEnv.Env.TraversePath().Load();
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