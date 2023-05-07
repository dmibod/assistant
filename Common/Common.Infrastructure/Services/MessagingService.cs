namespace Common.Infrastructure.Services;

using System.Collections.Concurrent;
using System.Reflection;
using Common.Core.Messaging;
using Common.Core.Messaging.Models;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Messaging.TypesProvider;
using Common.Core.Utils;
using Common.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;

public class MessagingService : BaseMessagingService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<MessagingService> logger;

    public MessagingService(
        IServiceProvider serviceProvider,
        IHandlerTypesProvider handlerTypesProvider,
        ITopicResolver topicResolver,
        IConnection connection,
        ILogger<MessagingService> logger) : base(handlerTypesProvider, topicResolver, connection)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override void LogMessage(string message)
    {
        this.logger.LogInformation(message);
    }

    protected override void LogError(string error)
    {
        this.logger.LogError(error);
    }

    protected override Task HandleAsync(object message)
    {
        var tenantAware = message as ITenantAware;

        return this.serviceProvider.ExecuteAsync(tenantAware?.Tenant ?? Identity.System, scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(TypeCache.GetHandlerType(message));

            var method = MethodCache.GetMethod(message);

            var parameters = new []
            {
                message
            };
            
            return (method.Invoke(service, parameters) as Task)!;
        });
    }
}

internal static class TypeCache
{
    private static readonly Type Mht = typeof(IMessageHandler<>);
    private static readonly IDictionary<Type, Type> Cache = new ConcurrentDictionary<Type, Type>();

    public static Type GetHandlerType(object message)
    {
        var messageType = message.GetType();

        if (Cache.TryGetValue(messageType, out var type))
        {
            return type;
        }
        
        Cache.Add(messageType, messageType.GenericTypeFrom(Mht));

        return Cache[messageType];
    }
}

internal static class MethodCache
{
    private const string MethodName = nameof(IMessageHandler<object>.HandleAsync);
    private static readonly IDictionary<Type, MethodInfo> Cache = new ConcurrentDictionary<Type, MethodInfo>();

    public static MethodInfo GetMethod(object message)
    {
        var messageType = message.GetType();

        if (Cache.TryGetValue(messageType, out var method))
        {
            return method;
        }
        
        Cache.Add(messageType, TypeCache.GetHandlerType(message).GetMethod(MethodName)!);

        return Cache[messageType];
    }
}
