namespace HomeSellerServer.Interface;

public interface IRabbitMqProducer
{
    Task PublishAsync<T>(string queueName, T message);
}