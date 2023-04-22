namespace Assistant.Market.Core.Services;

public interface IBusService
{
    Task PublishAsync(string topic);
    Task PublishAsync<T>(string topic, T data);
}