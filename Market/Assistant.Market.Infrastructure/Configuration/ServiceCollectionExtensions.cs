namespace Assistant.Market.Infrastructure.Configuration;

using Assistant.Market.Core.Repositories;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Assistant.Market.Infrastructure.Services;
using Common.Core.Services;
using Common.Infrastructure.Configuration;
using KanbanApi.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using PolygonApi.Client.Configuration;

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
        services.AddSingleton<IBusService, BusService>();
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        var polygonSettings = configuration.GetSection("PolygonApiSettings").Get<PolygonApiSettings>();
        services.AddHttpClient<PolygonApi.Client.ApiClient>("PolygonApiClient", client =>
        {
            client.BaseAddress = new Uri(polygonSettings.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {polygonSettings.ApiKey}");
        });
        var kanbanSettings = configuration.GetSection("KanbanApiSettings").Get<KanbanApiSettings>();
        services.AddHttpClient<KanbanApi.Client.ApiClient>("KanbanApiClient", client =>
        {
            client.BaseAddress = new Uri(kanbanSettings.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {kanbanSettings.ApiKey}");
        });
        services.AddSingleton<IKanbanService, KanbanService>();
        services.AddSingleton<IMarketDataService, MarketDataService>();
        services.AddSingleton<IPublishingService, PublishingService>();
        services.AddSingleton<IRefreshService, RefreshService>();
        services.AddSingleton<IStockService, StockService>();
        services.AddSingleton<IStockRepository, StockRepository>();
        services.AddSingleton<IOptionService, OptionService>();
        services.AddSingleton<IOptionRepository, OptionRepository>();
        
        services.AddIdentityProvider();
        
        services.AddHostedService<RefreshStockTimerService>();
        services.AddHostedService<RefreshStockWorkerService>();
        services.AddHostedService<CleanDataTimerService>();
        services.AddHostedService<CleanDataWorkerService>();
        services.AddHostedService<MarketDataTimerService>();
        services.AddHostedService<MarketDataWorkerService>();
        services.AddHostedService<AddStockWorkerService>();

        return services;
    }
}