namespace Assistant.Tenant.Infrastructure.Services;

using System.Text;
using System.Text.Json;
using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;

public class AddPositionWorkerService : BaseWorkerService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<AddPositionWorkerService> logger;

    public AddPositionWorkerService(
        IServiceProvider serviceProvider, 
        IOptions<NatsSettings> options, 
        IConnection connection, 
        ILogger<AddPositionWorkerService> logger)
        : base(options.Value.AddTenantPositionTopic, connection)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override void DoWork(object? sender, MsgHandlerEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Message.Data);
        var data = JsonSerializer.Deserialize<TenantPosition>(json);

        this.serviceProvider.Execute(data.Tenant, scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IPositionService>();
            
            service.CreateOrUpdateAsync(data.AsPosition()).GetAwaiter().GetResult();    
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

internal class TenantPosition : Position
{
    public string Tenant { get; set; }
}

internal static class TenantPositionExtensions
{
    public static Position AsPosition(this TenantPosition position)
    {
        return new Position
        {
            Account = position.Account,
            Quantity = position.Quantity,
            Ticker = position.Ticker,
            Tag = position.Tag,
            Type = position.Type,
            AverageCost = position.AverageCost
        };
    }
}