using MessageClient.Types;
namespace MessageClient.Interfaces;

public record struct OrderId
{
    public Guid Id { get; init; }
}

public class OrderCreatedEvent
{
    public OrderId OrderId { get; set; }
    public Guid UserId { get; set; }
    public Double TotalAmount { get; set; }
}

public class QueueName
{
    public string Name { get; set; }
}

public interface IMessageClient
{
    public Task SubscribeAsync<T>(Action<T> handle);
    public Task SubscribeAsync<T>(MessageSubscription queueName, Action<T> handle);
    public Task PublishAsync<T>(T message);
    public Task EnqueueAsync<T>(QueueName queueName, T message);
}