namespace Common.Infrastructure.Configuration;

using Common.Core.Security;
using Common.Infrastructure.Security;
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
}