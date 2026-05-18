using EasyNetQ;
using MessageClient.Adapters;
using MessageClient.Configuration;
using MessageClient.Interfaces;

namespace MessageClient.Factories;

public static class RabbitMqFactory
{
    private static RabbitMqAdapter CreateAdapter(IMessageClientOptions options)
    {
        IBus bus = RabbitHutch.CreateBus(options.ConnectionString);
        return new RabbitMqAdapter(bus);
    }

    public static IMessageClient CreateMessageClient(IMessageClientOptions options)
    {
        RabbitMqAdapter adapter = CreateAdapter(options);
        return new Implementation.MessageClient(adapter);
    }
}