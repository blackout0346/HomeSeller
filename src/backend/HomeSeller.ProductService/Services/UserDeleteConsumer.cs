using HomeSeller.Shared;
using HomeSellerServer.Module;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace ProductService.Services;

public class UserDeleteConsumer : IConsumer<UserDeletedEvent>
{
    private readonly ProductDbContext _dbContext;
    private readonly ILogger<UserDeleteConsumer> _logger;
    public UserDeleteConsumer(ProductDbContext dbContext, ILogger<UserDeleteConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        
    }
    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var userId = context.Message.UserId;
        _logger.LogInformation($"Обработка удаления пользователя{userId}", userId);
        var products = await _dbContext.Products.Where(p => p.UserId == userId).ToListAsync(context.CancellationToken);
        foreach (var product in products)
        {
            product.isDeleted = true;
        }
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}