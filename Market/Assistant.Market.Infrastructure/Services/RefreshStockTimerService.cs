namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Services;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class RefreshStockTimerService : BaseTimerService
{
    private static readonly TimeSpan Lag = TimeSpan.FromHours(4);
    private readonly string refreshStockRequestTopic;
    private readonly IServiceProvider serviceProvider;
    private readonly IBusService busService;
    private readonly ILogger<RefreshStockTimerService> logger;

    public RefreshStockTimerService(
        IServiceProvider serviceProvider,
        IBusService busService,
        IOptions<NatsSettings> options,
        ILogger<RefreshStockTimerService> logger)
        : base(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(GetInitialDelay(10, 30)))
    {
        this.refreshStockRequestTopic = options.Value.RefreshStockRequestTopic;
        this.serviceProvider = serviceProvider;
        this.busService = busService;
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.LogMessage($"{this.ServiceName} is working...");

        this.serviceProvider.Execute("system", scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IStockService>();

            var ticker = service.FindOutdatedTickerAsync(Lag).Result;
            if (ticker != null)
            {
                this.busService.PublishAsync(this.refreshStockRequestTopic, ticker);
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