namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

public class RefreshDataTimerService : IHostedService, IDisposable
{
    private readonly TimeSpan interval = TimeSpan.FromMinutes(1);
    private readonly string refreshStockRequestTopic;
    private readonly IBusService busService;
    private readonly ILogger<RefreshDataTimerService> logger;
    private Timer? timer;

    public RefreshDataTimerService(IBusService busService, IOptions<NatsSettings> options, ILogger<RefreshDataTimerService> logger)
    {
        this.refreshStockRequestTopic = options.Value.RefreshStockRequestTopic;
        this.busService = busService;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(RefreshDataTimerService)} is starting...");

        this.timer = new Timer(DoWork, null, TimeSpan.FromSeconds(10), this.interval);

        this.logger.LogInformation($"{nameof(RefreshDataTimerService)} has started.");

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        this.logger.LogInformation($"{nameof(RefreshDataTimerService)} is working...");

        this.busService.PublishAsync(this.refreshStockRequestTopic);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(RefreshDataTimerService)} is stopping...");

        this.timer?.Change(Timeout.Infinite, 0);

        this.logger.LogInformation($"{nameof(RefreshDataTimerService)} has stopped.");

        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        this.timer?.Dispose();
    }
}