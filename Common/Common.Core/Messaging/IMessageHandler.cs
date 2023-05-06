namespace Common.Core.Messaging;

public interface IMessageHandler<in T>
{
    Task HandleAsync(T message);
}