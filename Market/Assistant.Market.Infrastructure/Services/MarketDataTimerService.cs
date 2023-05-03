namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Services;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class MarketDataTimerService : BaseTimerService
{
    private readonly IBusService busService;
    private readonly string publishMarketDataTopic;
    private readonly ILogger<MarketDataTimerService> logger;

    public MarketDataTimerService(IBusService busService, IOptions<NatsSettings> options, ILogger<MarketDataTimerService> logger) 
        : base(TimeSpan.FromHours(1), TimeSpan.FromHours(1))
    {
        this.busService = busService;
        this.publishMarketDataTopic = options.Value.PublishMarketDataTopic;
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.busService.PublishAsync(this.publishMarketDataTopic).GetAwaiter().GetResult();
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