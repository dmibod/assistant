namespace Common.Infrastructure.Services;

using System.Collections.Concurrent;
using System.Reflection;
using Common.Core.Messaging;
using Common.Core.Messaging.TenantResolver;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Messaging.TypesProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;

public class MessagingService : BaseMessagingService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ITenantResolver tenantResolver;
    private readonly ILogger<MessagingService> logger;

    public MessagingService(
        IServiceProvider serviceProvider,
        IHandlerTypesProvider handlerTypesProvider,
        ITopicResolver topicResolver,
        ITenantResolver tenantResolver,
        IConnection connection,
        ILogger<MessagingService> logger) : base(handlerTypesProvider, topicResolver, connection)
    {
        this.serviceProvider = serviceProvider;
        this.tenantResolver = tenantResolver;
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

    protected override Task HandleAsync(object message, Type serviceType)
    {
        return this.serviceProvider.ExecuteAsync(this.tenantResolver.Resolve(message), scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(serviceType);

            var method = MethodCache.GetMethod(serviceType);

            var parameters = new []
            {
                message
            };
            
            return (method.Invoke(service, parameters) as Task)!;
        });
    }
}

internal static class MethodCache
{
    private const string MethodName = nameof(IMessageHandler<object>.HandleAsync);
    private static readonly ConcurrentDictionary<Type, MethodInfo> Cache = new();

    public static MethodInfo GetMethod(Type serviceType)
    {
        return Cache.GetOrAdd(serviceType, serviceTypeArg => serviceTypeArg.GetMethod(MethodName)!);
    }
}
