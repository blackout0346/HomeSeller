using System.Text;
using System.Text.Json;
using HomeSellerServer.Interface;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

namespace HomeSellerServer.Service;

public class RabbitMqProducer : IRabbitMqProducer, IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection _connection;

    public RabbitMqProducer(IConfiguration config)
    {
        _factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:HostName"] ?? "localhost",
        };
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        _connection ??= await _factory.CreateConnectionAsync();
        await using var channel = await _connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queueName, true, false, false, arguments:null);
        var json =JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
        
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}