namespace Assistant.Tenant.Infrastructure.Configuration;

using Assistant.Tenant.Core.Messaging;
using Assistant.Tenant.Core.Repositories;
using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Repositories;
using Assistant.Tenant.Infrastructure.Services;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Services;
using Common.Core.Utils;
using Common.Infrastructure.Configuration;
using Common.Infrastructure.Services;
using KanbanApi.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)));

        var kanbanSettings = configuration.GetSection(nameof(KanbanApiSettings)).Get<KanbanApiSettings>();
        services.AddHttpClient("KanbanApiClient", client =>
        {
            client.BaseAddress = new Uri(kanbanSettings.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {kanbanSettings.ApiKey}");
        });

        services.AddIdentityProvider();

        services.AddScoped<IKanbanService, KanbanService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IPositionPublishingService, PositionPublishingService>();
        services.AddScoped<IWatchListService, WatchListService>();
        services.AddScoped<IPublishingService, PublishingService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<IMarketDataService, MarketDataService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITenantRepository, TenantRepository>();

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
                [TopicUtils.AsTopic(nameof(NatsSettings.PositionCreateTopic))] = natsSettings.PositionCreateTopic,
                [TopicUtils.AsTopic(nameof(NatsSettings.PositionRefreshTopic))] = natsSettings.PositionRefreshTopic,
                [TopicUtils.AsTopic(nameof(NatsSettings.PositionRemoveTopic))] = natsSettings.PositionRemoveTopic
            };

            return new MapTopicResolver(topics);
        });
        services.ConfigureMessaging(new[]
        {
            typeof(PositionRemoveMessageHandler).Assembly
        }, ServiceLifetime.Scoped);

        return services;
    }
}