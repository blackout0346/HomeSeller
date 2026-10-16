using System.Text;
using System.Text.Json;
using HomeSeller.Shared;
using HomeSellerServer.Module;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HomeSellerServer.Service;

public class UserSyncConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IChannel? _channel;

    public UserSyncConsumer(
        IServiceProvider serviceProvider,
        IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:HostName"]
                       ?? "localhost"
        };

        _connection =
            await factory.CreateConnectionAsync(stoppingToken);

        _channel = await _connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: "user.deleted",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);


        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(
                    ea.Body.ToArray());

                var userEvent =
                    JsonSerializer.Deserialize<UserDeletedEvent>(
                        json);

                if (userEvent == null ||
                    userEvent.UserId == Guid.Empty)
                {
                    await _channel.BasicNackAsync(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                using var scope =
                    _serviceProvider.CreateScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ProductDbContext>();

                var products = await dbContext.Products
                    .Where(p => p.UserId == userEvent.UserId)
                    .ToListAsync(stoppingToken);

                foreach (var product in products)
                {
                    product.isDeleted = true;
                }

                await dbContext.SaveChangesAsync(stoppingToken);


                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "user.deleted",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    public override async Task StopAsync(
        CancellationToken stoppingToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(stoppingToken);

        if (_connection != null)
            await _connection.CloseAsync(stoppingToken);

        await base.StopAsync(stoppingToken);
    }
}