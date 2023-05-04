namespace Common.Infrastructure.Services;

using Common.Core.Security;
using Common.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static void Execute(this IServiceProvider serviceProvider, string name, Action<IServiceScope> action)
    {
        using var scope = serviceProvider.CreateScope();

        var holder = scope.ServiceProvider.GetService<IIdentityHolder>();

        holder.Identity = new Identity(name);
        
        action(scope);
    }
}