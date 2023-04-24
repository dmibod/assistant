namespace Common.Core.Services;

public interface IBusService
{
    Task PublishAsync(string topic);
    
    Task PublishAsync(string topic, string data);
    
    Task PublishAsync<T>(string topic, T data);
}