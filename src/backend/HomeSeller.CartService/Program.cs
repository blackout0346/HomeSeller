using HomeSeller.CartService.Module;
using Microsoft.EntityFrameworkCore;


DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);
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