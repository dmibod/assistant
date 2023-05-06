namespace Common.Core.Messaging;

public interface IMessageHandlerTypesProvider
{
    IEnumerable<Type> HandlerTypes { get; }
}