namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("{DataPublishTopic}")]
public class DataPublishMessageHandler : IMessageHandler<DataPublishMessage>
{
    private readonly IPublishingService publishingService;
    private readonly ILogger<DataPublishMessageHandler> logger;

    public DataPublishMessageHandler(IPublishingService publishingService, ILogger<DataPublishMessageHandler> logger)
    {
        this.publishingService = publishingService;
        this.logger = logger;
    }

    public async Task HandleAsync(DataPublishMessage message)
    {
        this.logger.LogInformation("Received publish data message");

        if (message.MarketData)
        {
            await this.publishingService.PublishAsync();
        }

        if (message.OpenInterest)
        {
            await this.publishingService.PublishOpenInterestAsync();
        }
    }
}

public class DataPublishMessage
{
    public bool MarketData { get; set; }

    public bool OpenInterest { get; set; }
} 