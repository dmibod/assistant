namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using NATS.Client;

public class FeedWorkerService : IHostedService, IDisposable
{
    private readonly TimeSpan lag = TimeSpan.FromHours(4);
    private readonly IFeedService feedService;
    private readonly IConnection connection;
    private IAsyncSubscription subscription;
    private readonly ILogger<FeedWorkerService> logger;

    public FeedWorkerService(IFeedService feedService, IConnection connection, ILogger<FeedWorkerService> logger)
    {
        this.feedService = feedService;
        this.connection = connection;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(FeedWorkerService)} is starting...");

        this.subscription = this.connection.SubscribeAsync("feed.stock.request", this.TryDoWork);

        this.logger.LogInformation($"{nameof(FeedWorkerService)} has started.");

        return Task.CompletedTask;
    }

    private void TryDoWork(object? sender, MsgHandlerEventArgs args)
    {
        try
        {
            this.logger.LogInformation($"{nameof(FeedWorkerService)} is working...");

            this.DoWork();
        }
        catch (Exception e)
        {
            this.logger.LogError(e.Message);
        }
    }

    private void DoWork()
    {
        this.feedService.FeedAsync(this.lag);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(FeedWorkerService)} is stopping...");

        this.subscription.Unsubscribe();
        await this.subscription.DrainAsync();

        this.logger.LogInformation($"{nameof(FeedWorkerService)} has stopped.");
    }

    public virtual void Dispose()
    {
    }
}