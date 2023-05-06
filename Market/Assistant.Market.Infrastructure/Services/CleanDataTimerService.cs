namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Messaging;
using Common.Core.Services;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class CleanDataTimerService : BaseTimerService
{
    private readonly IBusService busService;
    private readonly string dataCleanTopic;
    private readonly ILogger<CleanDataTimerService> logger;

    public CleanDataTimerService(IBusService busService, ITopicResolver topicResolver, ILogger<CleanDataTimerService> logger) 
        : base(TimeSpan.FromHours(24), TimeSpan.FromSeconds(GetInitialDelay(10, 60)))
    {
        this.busService = busService;
        this.dataCleanTopic = topicResolver.Resolve("{DataCleanTopic}");
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.busService.PublishAsync(this.dataCleanTopic).GetAwaiter().GetResult();
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