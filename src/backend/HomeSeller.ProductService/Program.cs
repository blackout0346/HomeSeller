using System.Text.Json;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using HomeSellerServer.StoreService;
using Microsoft.EntityFrameworkCore;
using ProductService.Services;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<UserSyncConsumer>();
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