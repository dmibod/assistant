namespace Assistant.Tenant.Infrastructure.Services;

using Assistant.Tenant.Core.Messaging;
using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Security;
using Common.Core.Services;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ScheduleTimerService : BaseTimerService
{
    private readonly string scheduleTopic;
    private readonly IServiceProvider serviceProvider;
    private readonly IBusService busService;
    private readonly ILogger<ScheduleTimerService> logger;

    public ScheduleTimerService(
        IServiceProvider serviceProvider,
        IBusService busService,
        ITopicResolver topicResolver,
        ILogger<ScheduleTimerService> logger)
        : base(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(GetInitialDelay(60, 90)))
    {
        this.scheduleTopic = topicResolver.ResolveConfig(nameof(NatsSettings.ScheduleTopic));
        this.serviceProvider = serviceProvider;
        this.busService = busService;
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.serviceProvider.Execute(Identity.System, scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<ITenantService>();

            var tenants = service.FindAllTenantsAsync().Result;

            foreach (var tenant in tenants)
            {
                this.busService
                    .PublishAsync(this.scheduleTopic, new ScheduleMessage { Tenant = tenant })
                    .GetAwaiter()
                    .GetResult();
            }
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