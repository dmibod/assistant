namespace Assistant.Market.Infrastructure.Configuration;

using Assistant.Market.Core.Messaging;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Assistant.Market.Infrastructure.Services;
using Common.Core.Messaging.TenantResolver;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Services;
using Common.Core.Utils;
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
        services.Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)));

        var polygonSettings = configuration.GetSection(nameof(PolygonApiSettings)).Get<PolygonApiSettings>();
        services.AddHttpClient("PolygonApiClient", client =>
        {
            client.BaseAddress = new Uri(polygonSettings!.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {polygonSettings.ApiKey}");
        });
        
        var kanbanSettings = configuration.GetSection(nameof(KanbanApiSettings)).Get<KanbanApiSettings>();
        services.AddHttpClient("KanbanApiClient", client =>
        {
            client.BaseAddress = new Uri(kanbanSettings!.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {kanbanSettings.ApiKey}");
        });

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

        services.ConfigureMessaging(configuration);

        return services;
    }
    
    public static IServiceCollection ConfigureMessaging(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var natsSection = configuration.GetSection(nameof(NatsSettings));
        var natsSettings = natsSection.Get<NatsSettings>();
        services.AddNatsClient(options =>
        {
            options.User = natsSettings!.User;
            options.Password = natsSettings.Password;
            options.Url = natsSettings.Url;
        });
        services.Configure<NatsSettings>(natsSection);
        services.AddSingleton<IBusService, BusService>();
        services.AddSingleton<ITopicResolver, MapTopicResolver>(_ =>
        {
            var topics = new Dictionary<string, string>
            {
                [TopicUtils.AsTopic(nameof(NatsSettings.StockCreateTopic))] = natsSettings!.StockCreateTopic,
                [TopicUtils.AsTopic(nameof(NatsSettings.StockRefreshTopic))] = natsSettings.StockRefreshTopic,
                [TopicUtils.AsTopic(nameof(NatsSettings.DataCleanTopic))] = natsSettings.DataCleanTopic,
                [TopicUtils.AsTopic(nameof(NatsSettings.DataPublishTopic))] = natsSettings.DataPublishTopic
            };

            return new MapTopicResolver(topics);
        });
        services.ConfigureMessaging(new[]
        {
            typeof(StockCreateMessageHandler).Assembly
        }, ServiceLifetime.Scoped);
        services.AddSingleton<ITenantResolver, TenantAwareTenantResolver>();

        return services;
    }
}