namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using NATS.Client;

public class RefreshDataWorkerService : IHostedService, IDisposable
{
    private readonly TimeSpan lag = TimeSpan.FromHours(4);
    private readonly IRefreshService refreshService;
    private readonly IConnection connection;
    private readonly string refreshStockRequestTopic;
    private IAsyncSubscription subscription;
    private readonly ILogger<RefreshDataWorkerService> logger;

    public RefreshDataWorkerService(IRefreshService refreshService, IConnection connection, IOptions<NatsSettings> options, ILogger<RefreshDataWorkerService> logger)
    {
        this.refreshService = refreshService;
        this.connection = connection;
        this.refreshStockRequestTopic = options.Value.RefreshStockRequestTopic;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(RefreshDataWorkerService)} is starting...");

        this.subscription = this.connection.SubscribeAsync(this.refreshStockRequestTopic, this.TryDoWork);

        this.logger.LogInformation($"{nameof(RefreshDataWorkerService)} has started.");

        return Task.CompletedTask;
    }

    private void TryDoWork(object? sender, MsgHandlerEventArgs args)
    {
        try
        {
            this.logger.LogInformation($"{nameof(RefreshDataWorkerService)} is working...");

            this.DoWork();
        }
        catch (Exception e)
        {
            this.logger.LogError(e.Message);
        }
    }

    private void DoWork()
    {
        this.refreshService.RefreshAsync(this.lag);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(RefreshDataWorkerService)} is stopping...");

        this.subscription.Unsubscribe();
        await this.subscription.DrainAsync();

        this.logger.LogInformation($"{nameof(RefreshDataWorkerService)} has stopped.");
    }

    public virtual void Dispose()
    {
    }
}