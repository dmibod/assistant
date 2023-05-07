namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("{DataCleanTopic}")]
public class DataCleanMessageHandler : IMessageHandler<DataCleanMessage>
{
    private readonly IRefreshService refreshService;
    private readonly ILogger<DataCleanMessageHandler> logger;

    public DataCleanMessageHandler(IRefreshService refreshService, ILogger<DataCleanMessageHandler> logger)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    public Task HandleAsync(DataCleanMessage message)
    {
        this.logger.LogInformation("Received clean data message");
        
        return this.refreshService.CleanAsync(DateTime.UtcNow);
    }
}

public class DataCleanMessage
{
}