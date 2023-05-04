namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class MarketDataWorkerService : BaseWorkerService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<MarketDataWorkerService> logger;

    public MarketDataWorkerService(IServiceProvider serviceProvider, IConnection connection,
        IOptions<NatsSettings> options, ILogger<MarketDataWorkerService> logger)
        : base(options.Value.PublishMarketDataTopic, connection)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        this.serviceProvider.Execute("system", scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IPublishingService>();

            service.PublishAsync().GetAwaiter().GetResult();
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