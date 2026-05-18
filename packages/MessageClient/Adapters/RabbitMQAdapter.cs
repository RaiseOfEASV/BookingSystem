using EasyNetQ;
using MessageClient.Interfaces;
using MessageClient.Types;

namespace MessageClient.Adapters;

public class RabbitMqAdapter(IBus bus) : IAdapter
{
    private readonly Dictionary<string, SubscriptionResult> _subscriptions = new Dictionary<string, SubscriptionResult>();

    public async Task PublishAsync<T>(T message, CancellationToken token = default)
    {
        Console.WriteLine($"Publishing message: {message}");
        await bus.PubSub.PublishAsync(message, token);
    }

    public async Task SubscribeAsync<T>(MessageSubscription subscription, Action<T> handler,
        CancellationToken token = default)
    {
        var subscriptionHandle = await bus.PubSub.SubscribeAsync<T>(
            subscription.Subscription, handler, token);
        _subscriptions.Add(subscription.Subscription, subscriptionHandle);
    }

    public Task UnsubscribeAsync<T>(MessageSubscription subscription, CancellationToken token = default)
    {
        if (!_subscriptions.Remove(subscription.Subscription, out var subscriptionHandle));
        return Task.CompletedTask;
    }
}