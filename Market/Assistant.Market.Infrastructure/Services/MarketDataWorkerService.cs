namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class MarketDataWorkerService : BaseWorkerService
{
    private readonly IPublishingService publishingService;
    private readonly ILogger<MarketDataWorkerService> logger;

    public MarketDataWorkerService(IPublishingService publishingService, IConnection connection,
        IOptions<NatsSettings> options, ILogger<MarketDataWorkerService> logger)
        : base(connection, options.Value.PublishMarketDataTopic)
    {
        this.publishingService = publishingService;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        this.publishingService.PublishAsync().GetAwaiter().GetResult();
    }

    protected override void LogMessage(string message)
    {
        this.logger.LogInformation(message);
    }

    protected override void LogError(string error)
    {
        this.logger.LogError(error);
    }
}