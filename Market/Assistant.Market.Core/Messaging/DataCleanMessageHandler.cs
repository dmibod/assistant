namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{DataCleanTopic}")]
public class DataCleanMessageHandler : IMessageHandler<EmptyMessage>
{
    private readonly IRefreshService refreshService;
    private readonly ILogger<DataCleanMessageHandler> logger;

    public DataCleanMessageHandler(IRefreshService refreshService, ILogger<DataCleanMessageHandler> logger)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    public Task HandleAsync(EmptyMessage message)
    {
        this.logger.LogInformation("Received clean data message");
        
        return this.refreshService.CleanAsync(DateTime.UtcNow);
    }
}