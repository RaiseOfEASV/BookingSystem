using MessageClient.Types;

namespace MessageClient.Interfaces;

public interface IAdapter
{
    Task SubscribeAsync<T>(MessageSubscription subscription, Action<T> handler,
        CancellationToken token = default);
    Task UnsubscribeAsync<T>(MessageSubscription subscription, CancellationToken token = default);
    Task PublishAsync<T>(T message, CancellationToken token = default);
    
}