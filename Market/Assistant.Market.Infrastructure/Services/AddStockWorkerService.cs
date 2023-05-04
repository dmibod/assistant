namespace Assistant.Market.Infrastructure.Services;

using System.Text;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class AddStockWorkerService : BaseWorkerService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<AddStockWorkerService> logger;

    public AddStockWorkerService(IServiceProvider serviceProvider, IConnection connection,
        IOptions<NatsSettings> options, ILogger<AddStockWorkerService> logger)
        : base(options.Value.AddStockRequestTopic, connection)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        var ticker = Encoding.UTF8.GetString(args.Message.Data);

        this.serviceProvider.Execute("system", scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IStockService>();
            
            service.GetOrCreateAsync(ticker).GetAwaiter().GetResult();
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