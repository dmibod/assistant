namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{DataPublishTopic}")]
public class DataPublishMessageHandler : IMessageHandler<EmptyMessage>
{
    private readonly IPublishingService publishingService;
    private readonly Logger<DataPublishMessageHandler> logger;

    public DataPublishMessageHandler(IPublishingService publishingService, Logger<DataPublishMessageHandler> logger)
    {
        this.publishingService = publishingService;
        this.logger = logger;
    }

    public Task HandleAsync(EmptyMessage message)
    {
        this.logger.LogInformation("Received publish data message");
        
        return this.publishingService.PublishAsync();
    }
}