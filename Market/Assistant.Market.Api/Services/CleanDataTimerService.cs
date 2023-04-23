namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

public class CleanDataTimerService : BaseTimerService
{
    private readonly string cleanDataRequestTopic;
    private readonly IBusService busService;
    private readonly ILogger<CleanDataTimerService> logger;

    public CleanDataTimerService(IBusService busService, IOptions<NatsSettings> options, ILogger<CleanDataTimerService> logger) 
        : base(TimeSpan.FromHours(24), TimeSpan.FromSeconds(GetInitialDelay(10, 60)))
    {
        this.cleanDataRequestTopic = options.Value.CleanDataRequestTopic;
        this.busService = busService;
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.LogMessage($"{this.ServiceName} is working...");

        this.busService.PublishAsync(this.cleanDataRequestTopic);
    }

    protected override void LogMessage(string message)
    {
        this.logger.LogInformation(message);
    }
}