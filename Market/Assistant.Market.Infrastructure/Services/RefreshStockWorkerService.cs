namespace Assistant.Market.Infrastructure.Services;

using System.Text;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class RefreshStockWorkerService : BaseWorkerService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<RefreshStockWorkerService> logger;

    public RefreshStockWorkerService(
        IServiceProvider serviceProvider,
        IConnection connection,
        IOptions<NatsSettings> options,
        ILogger<RefreshStockWorkerService> logger)
        : base(options.Value.RefreshStockRequestTopic, connection)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        var ticker = Encoding.UTF8.GetString(args.Message.Data);

        this.serviceProvider.Execute("system", scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IRefreshService>();

            service.UpdateStockAsync(ticker);
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