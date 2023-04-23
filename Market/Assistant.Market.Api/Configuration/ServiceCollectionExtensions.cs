namespace Assistant.Market.Api.Configuration;

using Assistant.Market.Api.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<RefreshDataTimerService>();
        services.AddHostedService<RefreshDataWorkerService>();
        services.AddHostedService<CleanDataTimerService>();
        services.AddHostedService<CleanDataWorkerService>();
        services.AddHostedService<MarketDataWorkerService>();
        services.AddHostedService<AddStockWorkerService>();

        return services;
    }
}