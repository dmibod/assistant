namespace Common.Infrastructure.Configuration;

using Common.Core.Security;
using Common.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityProvider(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IIdentityProvider, IdentityProvider>();

        return services;
    }
}