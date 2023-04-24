namespace Assistant.Market.Infrastructure.Services;

using System.Text;
using System.Text.Json;
using Common.Core.Services;
using Microsoft.Extensions.Logging;
using NATS.Client;

public class BusService : IBusService
{
    private readonly IConnection connection;
    private readonly ILogger<BusService> logger;

    public BusService(IConnection connection, ILogger<BusService> logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    public Task PublishAsync(string topic)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.PublishAsync), topic);

        this.connection.Publish(new Msg(topic));
        
        return Task.CompletedTask;
    }

    public Task PublishAsync(string topic, string data)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.PublishAsync), topic);

        this.connection.Publish(topic, Encoding.UTF8.GetBytes(data));
        
        return Task.CompletedTask;
    }

    public Task PublishAsync<T>(string topic, T data)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.PublishAsync), topic);

        this.connection.Publish(topic, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)));
        
        return Task.CompletedTask;
    }
}