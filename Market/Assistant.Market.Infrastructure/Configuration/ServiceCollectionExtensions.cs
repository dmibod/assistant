﻿namespace Assistant.Market.Infrastructure.Configuration;

using Assistant.Market.Core.Repositories;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Assistant.Market.Infrastructure.Services;
using Common.Core.Services;
using Common.Infrastructure.Configuration;
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
        services.AddHttpClient<PolygonApi.Client.ApiClient>("PolygonApiClient");
        services.AddHttpClient<KanbanApi.Client.ApiClient>("KanbanApiClient", client =>
        {
            client.BaseAddress = new Uri("http://assistant.dmitrybodnar.com:8080/v1/api/");
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
        
        services.AddHostedService<RefreshDataTimerService>();
        services.AddHostedService<RefreshDataWorkerService>();
        services.AddHostedService<CleanDataTimerService>();
        services.AddHostedService<CleanDataWorkerService>();
        services.AddHostedService<MarketDataTimerService>();
        services.AddHostedService<MarketDataWorkerService>();
        services.AddHostedService<AddStockWorkerService>();

        return services;
    }
}