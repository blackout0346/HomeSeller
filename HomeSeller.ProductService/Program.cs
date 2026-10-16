using HomeSellerServer.Interface;
using HomeSellerServer.Module;
using HomeSellerServer.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});
builder.Services.AddSingleton<IProductService, ProductService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();