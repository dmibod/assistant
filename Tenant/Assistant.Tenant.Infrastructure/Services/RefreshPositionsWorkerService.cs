namespace Assistant.Tenant.Infrastructure.Services;

using System.Text;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class RefreshPositionsWorkerService : BaseWorkerService
{
    private readonly ILogger<RefreshPositionsWorkerService> logger;

    public RefreshPositionsWorkerService(IConnection connection,
        IOptions<NatsSettings> options, ILogger<RefreshPositionsWorkerService> logger)
        : base(connection, options.Value.RefreshTenantPositionTopic)
    {
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        var tenant = Encoding.UTF8.GetString(args.Message.Data);

        this.LogMessage($"Received positions refresh message for {tenant}");
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