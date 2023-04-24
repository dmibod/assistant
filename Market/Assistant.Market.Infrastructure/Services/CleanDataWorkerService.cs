namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class CleanDataWorkerService : BaseWorkerService
{
    private readonly IRefreshService refreshService;
    private readonly ILogger<CleanDataWorkerService> logger;

    public CleanDataWorkerService(IRefreshService refreshService, IConnection connection, IOptions<NatsSettings> options, ILogger<CleanDataWorkerService> logger) 
        : base(connection, options.Value.CleanDataRequestTopic)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        this.refreshService.CleanAsync(DateTime.UtcNow);
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