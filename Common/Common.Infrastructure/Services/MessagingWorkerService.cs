namespace Common.Infrastructure.Services;

using System.Collections.Concurrent;
using System.Reflection;
using Common.Core.Messaging;
using Common.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;

public class MessagingWorkerService : BaseMessagingService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IMessageHandlerTypesProvider messageHandlerTypesProvider;
    private readonly ITopicResolver topicResolver;
    private readonly ILogger<MessagingWorkerService> logger;

    public MessagingWorkerService(
        IServiceProvider serviceProvider,
        IMessageHandlerTypesProvider messageHandlerTypesProvider,
        ITopicResolver topicResolver,
        IConnection connection,
        ILogger<MessagingWorkerService> logger) : base(connection)
    {
        this.serviceProvider = serviceProvider;
        this.messageHandlerTypesProvider = messageHandlerTypesProvider;
        this.topicResolver = topicResolver;
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

    protected override string ResolveTopic(string topic)
    {
        return this.topicResolver.Resolve(topic);
    }

    protected override Task HandleAsync(object message)
    {
        var tenantAware = message as ITenantAware;

        return this.serviceProvider.ExecuteAsync(tenantAware?.Tenant ?? "system", scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(TypeCache.GetHandlerType(message));

            var method = MethodCache.GetMethod(message);

            var parameters = new []
            {
                message
            };
            
            return method.Invoke(service, parameters) as Task;
        });
    }

    protected override void SubscribeHandlers(Action<Type> action)
    {
        foreach (var messageHandlerType in this.messageHandlerTypesProvider.HandlerTypes)
        {
            action(messageHandlerType);
        }
    }
}

internal static class TypeCache
{
    private static readonly IDictionary<Type, Type> Cache = new ConcurrentDictionary<Type, Type>();

    public static Type GetHandlerType(object message)
    {
        var messageType = message.GetType();

        if (Cache.TryGetValue(messageType, out var type))
        {
            return type;
        }
        
        Cache.Add(messageType, messageType.GenericTypeFrom(typeof(IMessageHandler<>)));

        return Cache[messageType];
    }
}

internal static class MethodCache
{
    private static readonly IDictionary<Type, MethodInfo> Cache = new ConcurrentDictionary<Type, MethodInfo>();

    public static MethodInfo GetMethod(object message)
    {
        var messageType = message.GetType();

        if (Cache.TryGetValue(messageType, out var method))
        {
            return method;
        }
        
        Cache.Add(messageType, TypeCache.GetHandlerType(message).GetMethod("HandleAsync")!);

        return Cache[messageType];
    }
}
