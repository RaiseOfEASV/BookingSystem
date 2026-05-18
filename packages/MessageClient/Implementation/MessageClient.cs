using MessageClient.Interfaces;
using MessageClient.Types;

namespace MessageClient.Implementation;

public class MessageClient(IAdapter adapter) : IMessageClient
{
    public Task UnsubscribeAsync<T>(MessageSubscription subscription, CancellationToken token = default)
    {
        return adapter.UnsubscribeAsync<T>(subscription, token);
    }

    public Task SubscribeAsync<T>(Action<T> handle)
    {
        return adapter.SubscribeAsync(new MessageSubscription(nameof(T)), handle);
    }

    public Task SubscribeAsync<T>(MessageSubscription queueName, Action<T> handle)
    {
        return adapter.SubscribeAsync(queueName, handle);
    }

    public Task SubscribeAsync<T>(QueueName queueName, Action<T> handler)
    {
        throw new NotImplementedException();
    }

    public Task PublishAsync<T>(T message)
    {
        return adapter.PublishAsync(message);
    }

    public Task EnqueueAsync<T>(QueueName queueName, T message)
    {
        throw new NotImplementedException();
    }

    public Task EnqueueAsync<T>(QueueName queueName, T message, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}