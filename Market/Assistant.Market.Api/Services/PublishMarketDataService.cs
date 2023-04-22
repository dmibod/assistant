namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using NATS.Client;

public class PublishMarketDataService : IHostedService, IDisposable
{
    private readonly IPublishingService publishingService;
    private readonly IConnection connection;
    private readonly string publishMarketDataTopic;
    private IAsyncSubscription subscription;
    private readonly ILogger<PublishMarketDataService> logger;

    public PublishMarketDataService(IPublishingService publishingService, IConnection connection, IOptions<NatsSettings> options, ILogger<PublishMarketDataService> logger)
    {
        this.publishingService = publishingService;
        this.connection = connection;
        this.publishMarketDataTopic = options.Value.PublishMarketDataTopic;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(PublishMarketDataService)} is starting...");

        this.subscription = this.connection.SubscribeAsync(this.publishMarketDataTopic, this.TryDoWork);

        this.logger.LogInformation($"{nameof(PublishMarketDataService)} has started.");

        return Task.CompletedTask;
    }

    private void TryDoWork(object? sender, MsgHandlerEventArgs args)
    {
        try
        {
            this.logger.LogInformation($"{nameof(PublishMarketDataService)} is working...");

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
        this.logger.LogInformation($"{nameof(PublishMarketDataService)} is stopping...");

        this.subscription.Unsubscribe();
        await this.subscription.DrainAsync();

        this.logger.LogInformation($"{nameof(PublishMarketDataService)} has stopped.");
    }

    public virtual void Dispose()
    {
    }
}