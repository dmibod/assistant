namespace Assistant.Market.Infrastructure.Configuration;

using Assistant.Market.Core.Repositories;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Assistant.Market.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddNatsClient(options =>
        {
            var settings = configuration.GetSection("NatsSettings").Get<NatsSettings>();
            options.User = settings.User;
            options.Password = settings.Password;
            options.Url = settings.Url;
        });
        services.Configure<NatsSettings>(configuration.GetSection("NatsSettings"));
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
        services.AddSingleton<IOptionService, OptionService>();
        services.AddSingleton<IOptionRepository, OptionRepository>();

        return services;
    }
}