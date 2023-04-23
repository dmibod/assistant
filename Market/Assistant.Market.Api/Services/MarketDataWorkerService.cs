namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using NATS.Client;

public class MarketDataWorkerService : IHostedService, IDisposable
{
    private readonly IPublishingService publishingService;
    private readonly IConnection connection;
    private readonly string publishMarketDataTopic;
    private IAsyncSubscription subscription;
    private readonly ILogger<MarketDataWorkerService> logger;

    public MarketDataWorkerService(IPublishingService publishingService, IConnection connection, IOptions<NatsSettings> options, ILogger<MarketDataWorkerService> logger)
    {
        this.publishingService = publishingService;
        this.connection = connection;
        this.publishMarketDataTopic = options.Value.PublishMarketDataTopic;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(MarketDataWorkerService)} is starting...");

        this.subscription = this.connection.SubscribeAsync(this.publishMarketDataTopic, this.TryDoWork);

        this.logger.LogInformation($"{nameof(MarketDataWorkerService)} has started.");

        return Task.CompletedTask;
    }

    private void TryDoWork(object? sender, MsgHandlerEventArgs args)
    {
        try
        {
            this.logger.LogInformation($"{nameof(MarketDataWorkerService)} is working...");

            this.DoWork();
        }
        catch (Exception e)
        {
            this.logger.LogError(e.Message);
        }
    }

    private void DoWork()
    {
        this.publishingService.PublishAsync().GetAwaiter().GetResult();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(MarketDataWorkerService)} is stopping...");

        this.subscription.Unsubscribe();
        await this.subscription.DrainAsync();

        this.logger.LogInformation($"{nameof(MarketDataWorkerService)} has stopped.");
    }

    public virtual void Dispose()
    {
    }
}