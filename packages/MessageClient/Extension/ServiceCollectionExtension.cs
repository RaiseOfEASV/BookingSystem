using MessageClient.Configuration;
using MessageClient.Factories;
using MessageClient.Implementation;
using MessageClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace MessageClient.Extension;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRabbitMqMessageClient(this IServiceCollection services, IMessageClientOptions options)
    {
        IMessageClient messageClient = RabbitMqFactory.CreateMessageClient(options);
        services.AddSingleton(messageClient);
        return services;
    }

    public static IServiceCollection AddRabbitMqMessageClient(this IServiceCollection services, IMessageClientOptions options, MessageHandlerOptions handlerOptions)
    {
        services.AddRabbitMqMessageClient(options);
        services.AddSingleton<MessageHandlerRegistry>();
        services.AddHostedService<MessageBackgroundService>();
        
        return services;
    }
}