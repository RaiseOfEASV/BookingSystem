using System.Linq.Expressions;
using System.Reflection;
using MessageClient.Interfaces;
using MessageClient.Types;
using Microsoft.Extensions.DependencyInjection;

namespace MessageClient.Implementation;

public class MessageHandlerRegistry
{
    private readonly IServiceProvider _serviceProvider;

    public MessageHandlerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void RegisterAllHandlers(IMessageClient messageClient, string subscriptionPrefix = "")
    {
        // Scan all loaded assemblies. Guard each assembly individually so a broken
        // type (e.g. SqlGuidCaster in Microsoft.Data.SqlClient on Linux) does not
        // abort the entire scan — use the partially-loaded types from the exception.
        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try   { return a.GetTypes(); }
                catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null)!; }
            })
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in allTypes)
        {
            var messageHandlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));

            foreach (var handlerInterface in messageHandlerInterfaces)
            {
                var messageType = handlerInterface.GetGenericArguments()[0];
                Console.WriteLine($"Found handler {type.Name} for message type {messageType.Name}");

                var subscribeAsyncMethods = typeof(IMessageClient).GetMethods()
                    .Where(m => m.Name == nameof(IMessageClient.SubscribeAsync)).ToList();

                var subscribeAsyncMethod = subscribeAsyncMethods.FirstOrDefault(m =>
                {
                    var parameters = m.GetParameters();
                    return parameters.Length == 2
                           && parameters[0].ParameterType == typeof(MessageSubscription)
                           && parameters[1].Name.StartsWith("handle");
                });

                if (subscribeAsyncMethod == null)
                {
                    Console.WriteLine($"Could not find matching SubscribeAsync<T> method for message type {messageType.Name}");
                    continue;
                }

                var genericSubscribeMethod = subscribeAsyncMethod.MakeGenericMethod(messageType);

                var methodInfo = handlerInterface.GetMethod("Handle");
                if (methodInfo == null)
                {
                    Console.WriteLine($"No Handle method found in {handlerInterface.Name}");
                    continue;
                }

                // Capture handler type for the closure.
                // Create a fresh DI scope per message so scoped services (DbContext,
                // repositories) are resolved and disposed correctly for each message.
                var handlerType = type;
                Func<object, Task> typedFunc = async msg =>
                {
                    using var scope           = _serviceProvider.CreateScope();
                    var       handlerInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType);
                    await (Task)methodInfo.Invoke(handlerInstance, new object[] { msg, CancellationToken.None })!;
                };

                var param      = Expression.Parameter(messageType, "message");
                var expr       = Expression.Invoke(Expression.Constant(typedFunc), param);
                var lambdaType = typeof(Action<>).MakeGenericType(messageType);
                var lambda     = Expression.Lambda(lambdaType, expr, param).Compile();

                var subscriptionId = new MessageSubscription(
                    $"{subscriptionPrefix}_{type.FullName}_{messageType.Name}");

                genericSubscribeMethod.Invoke(
                    obj:        messageClient,
                    parameters: new object[] { subscriptionId, lambda });
            }
        }
    }
}
