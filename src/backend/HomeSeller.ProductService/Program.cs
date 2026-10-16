using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using HomeSellerServer.Service;
using HomeSellerServer.StoreService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<UserSyncConsumer>();
builder.Services.AddTransient<IProductStore, ProductStore>();
builder.Services.AddTransient<IProductService, ProductService>();


builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();