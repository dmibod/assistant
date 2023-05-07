namespace Common.Infrastructure.Services;

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Messaging.TypesProvider;
using Common.Core.Utils;
using NATS.Client;

public abstract class BaseMessagingService : BaseHostedService
{
    private readonly IHandlerTypesProvider handlerTypesProvider;
    private readonly ITopicResolver topicResolver;
    private readonly IConnection connection;

    private readonly IDictionary<string, IAsyncSubscription> subscriptions =
        new ConcurrentDictionary<string, IAsyncSubscription>();

    protected BaseMessagingService(
        IHandlerTypesProvider handlerTypesProvider,
        ITopicResolver topicResolver,
        IConnection connection)
    {
        this.handlerTypesProvider = handlerTypesProvider;
        this.topicResolver = topicResolver;
        this.connection = connection;
    }

    protected abstract Task HandleAsync(object message, Type serviceType);

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mht = typeof(IMessageHandler<>);

        foreach (var handlerType in this.handlerTypesProvider.HandlerTypes)
        {
            var topic = this.GetHandlerTopic(handlerType);

            var messageType = handlerType.GenericArgumentOf(mht);

            var subscription =
                this.connection.SubscribeAsync(topic, (_, args) => this.OnMessage(messageType, args.Message, messageType.GenericTypeFrom(mht)));

            this.subscriptions.Add(topic, subscription);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var subscription in this.subscriptions.Values)
        {
            subscription.Unsubscribe();
            await subscription.DrainAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private void OnMessage(Type payloadType, Msg message, Type serviceType)
    {
        try
        {
            var json = Encoding.UTF8.GetString(message.Data);

            var payload = string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize(json, payloadType);

            this.HandleAsync(payload!, serviceType).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            this.LogError(e.Message);
        }
    }

    private string GetHandlerTopic(Type handlerType)
    {
        var topic = handlerType.GetAttribute<HandlerAttribute>().Topic;

        return this.topicResolver.Resolve(topic);
    }
}