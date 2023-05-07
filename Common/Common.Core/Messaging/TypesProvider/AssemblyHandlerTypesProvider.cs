namespace Common.Core.Messaging.TypesProvider;

using System.Reflection;
using Common.Core.Messaging.Attributes;
using Common.Core.Utils;

public class AssemblyHandlerTypesProvider : IHandlerTypesProvider
{
    public AssemblyHandlerTypesProvider(IEnumerable<Assembly> assemblies)
    {
        var mht = typeof(IMessageHandler<>);
        
        this.HandlerTypes = (assemblies ?? Array.Empty<Assembly>())
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsGenericOf(mht) && type.HasAttribute<HandlerAttribute>() && type.GetAttribute<HandlerAttribute>().Enabled)
            .ToArray();
    }

    public IEnumerable<Type> HandlerTypes { get; }
}