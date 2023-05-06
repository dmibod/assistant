namespace Assistant.Tenant.Infrastructure.Services;

using System.Text;
using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class RefreshPositionsWorkerService : BaseWorkerService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<RefreshPositionsWorkerService> logger;

    public RefreshPositionsWorkerService(
        IServiceProvider serviceProvider, 
        IOptions<NatsSettings> options,
        IConnection connection, 
        ILogger<RefreshPositionsWorkerService> logger)
        : base(options.Value.RefreshTenantPositionTopic, connection)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        var tenant = Encoding.UTF8.GetString(args.Message.Data);

        this.LogMessage($"Received positions refresh message for {tenant}");
        
        this.serviceProvider.Execute(tenant, scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IPositionPublishingService>();
            
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