namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using NATS.Client;

public class CleanDataWorkerService : IHostedService, IDisposable
{
    private readonly IRefreshService refreshService;
    private readonly IConnection connection;
    private readonly string cleanDataRequestTopic;
    private IAsyncSubscription subscription;
    private readonly ILogger<CleanDataWorkerService> logger;

    public CleanDataWorkerService(IRefreshService refreshService, IConnection connection, IOptions<NatsSettings> options, ILogger<CleanDataWorkerService> logger)
    {
        this.refreshService = refreshService;
        this.connection = connection;
        this.cleanDataRequestTopic = options.Value.CleanDataRequestTopic;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(CleanDataWorkerService)} is starting...");

        this.subscription = this.connection.SubscribeAsync(this.cleanDataRequestTopic, this.TryDoWork);

        this.logger.LogInformation($"{nameof(CleanDataWorkerService)} has started.");

        return Task.CompletedTask;
    }

    private void TryDoWork(object? sender, MsgHandlerEventArgs args)
    {
        try
        {
            this.logger.LogInformation($"{nameof(CleanDataWorkerService)} is working...");

            this.DoWork();
        }
        catch (Exception e)
        {
            this.logger.LogError(e.Message);
        }
    }

    private void DoWork()
    {
        this.refreshService.CleanAsync(DateTime.UtcNow);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(CleanDataWorkerService)} is stopping...");

        this.subscription.Unsubscribe();
        await this.subscription.DrainAsync();

        this.logger.LogInformation($"{nameof(CleanDataWorkerService)} has stopped.");
    }

    public virtual void Dispose()
    {
    }
}