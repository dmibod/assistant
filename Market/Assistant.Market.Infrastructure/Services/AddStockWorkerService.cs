namespace Assistant.Market.Infrastructure.Services;

using System.Text;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class AddStockWorkerService : BaseWorkerService
{
    private readonly IStockService stockService;
    private readonly ILogger<AddStockWorkerService> logger;

    public AddStockWorkerService(IStockService stockService, IConnection connection,
        IOptions<NatsSettings> options, ILogger<AddStockWorkerService> logger)
        : base(connection, options.Value.AddStockRequestTopic)
    {
        this.stockService = stockService;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        var ticker = Encoding.UTF8.GetString(args.Message.Data);
        
        this.stockService.GetOrCreateAsync(ticker).GetAwaiter().GetResult();
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