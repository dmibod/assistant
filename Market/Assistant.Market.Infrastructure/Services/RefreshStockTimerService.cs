namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Messaging;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Services;
using Common.Infrastructure.Security;
using Common.Infrastructure.Services;
using Helper.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class RefreshStockTimerService : BaseTimerService
{
    private static readonly TimeSpan Lag = TimeSpan.FromHours(4);
    private readonly string stockRefreshTopic;
    private readonly IServiceProvider serviceProvider;
    private readonly IBusService busService;
    private readonly ILogger<RefreshStockTimerService> logger;

    public RefreshStockTimerService(
        IServiceProvider serviceProvider,
        IBusService busService,
        ITopicResolver topicResolver,
        ILogger<RefreshStockTimerService> logger)
        : base(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(GetInitialDelay(10, 30)))
    {
        this.stockRefreshTopic = topicResolver.ResolveConfig(nameof(NatsSettings.StockRefreshTopic));
        this.serviceProvider = serviceProvider;
        this.busService = busService;
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.serviceProvider.Execute(Identity.System, scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IStockService>();

            var ticker = service.FindOutdatedTickerAsync(Lag).Result;
            if (ticker != null)
            {
                this.busService
                    .PublishAsync(this.stockRefreshTopic, new StockRefreshMessage { Ticker = StockUtils.Format(ticker) })
                    .GetAwaiter()
                    .GetResult();
            }
        });
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