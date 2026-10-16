using System.Text.Json;
using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using HomeSellerServer.Service;
using HomeSellerServer.StoreService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
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
