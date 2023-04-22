namespace Assistant.Market.Infrastructure.Configuration;

using Assistant.Market.Core.Repositories;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Assistant.Market.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
        services.AddHttpClient<PolygonApi.Client.ApiClient>("PolygonApiClient");
        services.AddHttpClient<KanbanApi.Client.ApiClient>("KanbanApiClient", client =>
        {
            client.BaseAddress = new Uri("https://dmitrybodnar.com/v1/api/");
        });

        services.AddSingleton<IMarketDataService, MarketDataService>();
        services.AddSingleton<IFeedService, FeedService>();
        services.AddSingleton<IStockService, StockService>();
        services.AddSingleton<IStockRepository, StockRepository>();

        return services;
    }
}