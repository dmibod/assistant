namespace Common.Infrastructure.Services;

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Common.Core.Messaging;
using Common.Core.Utils;
using NATS.Client;

public abstract class BaseMessagingService : BaseHostedService
{
    private readonly IConnection connection;

    private readonly IDictionary<string, IAsyncSubscription> subscriptions =
        new ConcurrentDictionary<string, IAsyncSubscription>();

    protected BaseMessagingService(IConnection connection)
    {
        this.connection = connection;
    }

    protected abstract void SubscribeHandlers(Action<Type> action);

    protected abstract string ResolveTopic(string topic);
    
    protected abstract Task HandleAsync(object message);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.SubscribeHandlers(handlerType =>
        {
            var topic = this.ResolveTopic(GetTopicFromTypeAttribute(handlerType));
            var messageType = handlerType.GenericParameterOf(typeof(IMessageHandler<>));
            
            var subscription = this.connection.SubscribeAsync(topic, (sender, args) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(args.Message.Data);
                    var message = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize(json, messageType);

                    this.HandleAsync(message).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    this.LogError(e.Message);
                }
            });
            
            this.subscriptions.Add(topic, subscription);
        });

        return Task.CompletedTask;
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
    
    private static string GetTopicFromTypeAttribute(Type handlerType)
    {
        return handlerType.GetTypeAttribute<HandlerAttribute>().Topic;
    }
}