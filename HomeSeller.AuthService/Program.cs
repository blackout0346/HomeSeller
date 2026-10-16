using System.Text.Json;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!await context.Tags.AnyAsync())
    {
        var json = await File.ReadAllTextAsync("InitData.json");
        var tags = JsonSerializer.Deserialize<List<Tags>>(json);

        if (tags != null)
        {
            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
        }
    }
   
}
app.MapGet("/", () => "Hello World!");

app.Run();
