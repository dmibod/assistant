namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class RefreshDataWorkerService : BaseWorkerService
{
    private readonly TimeSpan lag = TimeSpan.FromHours(4);
    private readonly IRefreshService refreshService;
    private readonly ILogger<RefreshDataWorkerService> logger;

    public RefreshDataWorkerService(IRefreshService refreshService, IConnection connection,
        IOptions<NatsSettings> options, ILogger<RefreshDataWorkerService> logger)
        : base(connection, options.Value.RefreshStockRequestTopic)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        this.refreshService.RefreshAsync(this.lag);
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