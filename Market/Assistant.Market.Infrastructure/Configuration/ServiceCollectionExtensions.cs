namespace Assistant.Market.Infrastructure.Configuration;

using Assistant.Market.Core.Messaging;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Assistant.Market.Infrastructure.Services;
using Common.Core.Messaging;
using Common.Core.Services;
using Common.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using KanbanApi.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using PolygonApi.Client.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        var polygonSettings = configuration.GetSection("PolygonApiSettings").Get<PolygonApiSettings>();
        services.AddHttpClient("PolygonApiClient", client =>
        {
            client.BaseAddress = new Uri(polygonSettings.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {polygonSettings.ApiKey}");
        });
        
        var kanbanSettings = configuration.GetSection("KanbanApiSettings").Get<KanbanApiSettings>();
        services.AddHttpClient("KanbanApiClient", client =>
        {
            client.BaseAddress = new Uri(kanbanSettings.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {kanbanSettings.ApiKey}");
        });

        services.ConfigureMessaging(configuration);
        services.AddIdentityProvider();

        services.AddScoped<IKanbanService, KanbanService>();
        services.AddScoped<IMarketDataService, MarketDataService>();
        services.AddScoped<IPublishingService, PublishingService>();
        services.AddScoped<IRefreshService, RefreshService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IOptionService, OptionService>();
        services.AddScoped<IOptionRepository, OptionRepository>();
        services.AddScoped<IOptionChangeRepository, OptionChangeRepository>();
        
        services.AddHostedService<RefreshStockTimerService>();
        services.AddHostedService<CleanDataTimerService>();
        services.AddHostedService<MarketDataTimerService>();

        return services;
    }
    
    public static IServiceCollection ConfigureMessaging(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var settings = configuration.GetSection("NatsSettings").Get<NatsSettings>();

        services.AddNatsClient(options =>
        {
            options.User = settings.User;
            options.Password = settings.Password;
            options.Url = settings.Url;
        });
        services.Configure<NatsSettings>(configuration.GetSection("NatsSettings"));
        services.AddSingleton<IBusService, BusService>();

        services.AddSingleton<ITopicResolver, TopicResolver>(sp =>
        {
            var topics = new Dictionary<string, string>
            {
                ["{StockCreateTopic}"] = settings.StockCreateTopic,
                ["{StockRefreshTopic}"] = settings.StockRefreshTopic,
                ["{DataCleanTopic}"] = settings.DataCleanTopic,
                ["{DataPublishTopic}"] = settings.DataPublishTopic
            };

            return new TopicResolver(topics);
        });

        services.ConfigureMessaging(new[]
        {
            typeof(StockCreateMessageHandler).Assembly
        }, ServiceLifetime.Scoped);

        return services;
    }

}