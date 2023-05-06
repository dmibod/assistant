namespace Common.Infrastructure.Configuration;

using System.Reflection;
using Common.Core.Messaging;
using Common.Core.Security;
using Common.Core.Utils;
using Common.Infrastructure.Security;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityProvider(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IdentityManager>();
        services.AddScoped<IIdentityAccessor>(sp => sp.GetRequiredService<IdentityManager>());
        services.AddScoped<IIdentityHolder>(sp => sp.GetRequiredService<IdentityManager>());
        services.AddScoped<IIdentityProvider, IdentityProvider>();

        return services;
    }
    
    public static IServiceCollection ConfigureMessaging(
        this IServiceCollection services, 
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime)
    {
        var mht = typeof(IMessageHandler<>);

        var handlerTypesProvider = new MessageHandlerTypesProvider(assemblies); 
        var handlerTypes = handlerTypesProvider.HandlerTypes;

        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType.GenericParameterOf(mht).GenericTypeFrom(mht);
            
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                {
                    services.AddTransient(interfaceType, handlerType);
                } break;
                
                case ServiceLifetime.Scoped:
                {
                    services.AddScoped(interfaceType, handlerType);
                } break;
                
                case ServiceLifetime.Singleton:
                {
                    services.AddSingleton(interfaceType, handlerType);

                } break;
            }
        }

        services.AddSingleton<IMessageHandlerTypesProvider, MessageHandlerTypesProvider>(sp => handlerTypesProvider);
        services.AddHostedService<MessagingWorkerService>();

        return services;
    }
}