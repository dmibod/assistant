namespace Assistant.Market.Infrastructure.Services;

using System.Text;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class RefreshStockWorkerService : BaseWorkerService
{
    private readonly IRefreshService refreshService;
    private readonly ILogger<RefreshStockWorkerService> logger;

    public RefreshStockWorkerService(IRefreshService refreshService, IConnection connection,
        IOptions<NatsSettings> options, ILogger<RefreshStockWorkerService> logger)
        : base(connection, options.Value.RefreshStockRequestTopic)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        var ticker = Encoding.UTF8.GetString(args.Message.Data);
        
        this.refreshService.UpdateStockAsync(ticker);
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