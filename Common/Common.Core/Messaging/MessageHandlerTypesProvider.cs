namespace Common.Core.Messaging;

using System.Reflection;
using Common.Core.Utils;

public class MessageHandlerTypesProvider : IMessageHandlerTypesProvider
{
    public MessageHandlerTypesProvider(IEnumerable<Assembly> assemblies)
    {
        var mht = typeof(IMessageHandler<>);
        
        this.HandlerTypes = (assemblies ?? Array.Empty<Assembly>())
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsGenericOf(mht) && type.HasTypeAttribute<HandlerAttribute>() && type.GetTypeAttribute<HandlerAttribute>().Enabled)
            .ToArray();
    }

    public IEnumerable<Type> HandlerTypes { get; }
}