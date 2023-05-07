namespace Common.Core.Messaging.TypesProvider;

public interface IHandlerTypesProvider
{
    IEnumerable<Type> HandlerTypes { get; }
}