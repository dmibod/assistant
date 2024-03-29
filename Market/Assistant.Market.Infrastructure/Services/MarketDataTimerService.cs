﻿namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Messaging;
using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Services;
using Common.Infrastructure.Services;
using Microsoft.Extensions.Logging;

public class MarketDataTimerService : BaseTimerService
{
    private readonly IBusService busService;
    private readonly string dataPublishTopic;
    private readonly ILogger<MarketDataTimerService> logger;

    public MarketDataTimerService(IBusService busService, ITopicResolver topicResolver,
        ILogger<MarketDataTimerService> logger)
        : base(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(15))
    {
        this.busService = busService;
        this.dataPublishTopic = topicResolver.ResolveConfig(nameof(NatsSettings.DataPublishTopic));
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.busService.PublishAsync(this.dataPublishTopic, new DataPublishMessage
        {
            MarketData = true,
            OpenInterest = true
        }).GetAwaiter().GetResult();
    }

    protected override void LogMessage(string message)
    {
        this.logger.LogInformation(message);
    }

    protected override void LogError(string error)
    {
        this.logger.LogError(error);
    }
}