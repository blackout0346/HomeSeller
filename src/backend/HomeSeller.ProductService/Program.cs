using System.Text.Json;
using System.Threading.RateLimiting;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using HomeSellerServer.StoreService;
using MassTransit;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProductService.Module;
using ProductService.Services;

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
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserDeleteConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:HostName"]?? "localhost";
        cfg.Host(host, hostConfigurator =>
        {
            hostConfigurator.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            hostConfigurator.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddTransient<IProductStore, ProductStore>();
builder.Services.AddTransient<IProductService, ProductService.Services.ProductService>();


builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();


    await context.Database.MigrateAsync();

    if (!await context.Tags.AnyAsync())
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "initData.json");
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            var tags = JsonSerializer.Deserialize<List<Tags>>(json);

            if (tags != null && tags.Count > 0)
            {
                await context.Tags.AddRangeAsync(tags);
                await context.SaveChangesAsync();
            }
        }
    }
}

app.MapGet("/", () => "Hello World!");

app.Run();