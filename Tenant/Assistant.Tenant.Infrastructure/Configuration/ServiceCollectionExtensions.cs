namespace Assistant.Tenant.Infrastructure.Configuration;

using Assistant.Market.Infrastructure.Services;
using Assistant.Tenant.Core.Repositories;
using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Repositories;
using Assistant.Tenant.Infrastructure.Services;
using Common.Core.Services;
using Common.Infrastructure.Configuration;
using KanbanApi.Client.Configuration;
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
        services.AddSingleton<IBusService, BusService>();
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        var kanbanSettings = configuration.GetSection("KanbanApiSettings").Get<KanbanApiSettings>();
        services.AddHttpClient<KanbanApi.Client.ApiClient>("KanbanApiClient", client =>
        {
            client.BaseAddress = new Uri(kanbanSettings.ApiUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {kanbanSettings.ApiKey}");
        });
        services.AddIdentityProvider();
        services.AddSingleton<IKanbanService, KanbanService>();
        services.AddSingleton<ITenantService, TenantService>();
        services.AddSingleton<IPositionService, PositionService>();
        services.AddSingleton<IWatchListService, WatchListService>();
        services.AddSingleton<IPublishingService, PublishingService>();
        services.AddSingleton<ISuggestionService, SuggestionService>();
        services.AddSingleton<IMarketDataService, MarketDataService>();
        services.AddSingleton<ITenantRepository, TenantRepository>();
        
        services.AddHostedService<AddPositionWorkerService>();
        
        return services;
    }
}