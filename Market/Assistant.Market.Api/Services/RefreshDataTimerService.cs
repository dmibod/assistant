﻿namespace Assistant.Market.Api.Services;

using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

public class RefreshDataTimerService : BaseTimerService
{
    private readonly string refreshStockRequestTopic;
    private readonly IBusService busService;
    private readonly ILogger<RefreshDataTimerService> logger;

    public RefreshDataTimerService(IBusService busService, IOptions<NatsSettings> options, ILogger<RefreshDataTimerService> logger) 
        : base(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(GetInitialDelay(10, 30)))
    {
        this.refreshStockRequestTopic = options.Value.RefreshStockRequestTopic;
        this.busService = busService;
        this.logger = logger;
    }

    protected override void DoWork(object? state)
    {
        this.LogMessage($"{this.ServiceName} is working...");

        this.busService.PublishAsync(this.refreshStockRequestTopic);
    }

    protected override void LogMessage(string message)
    {
        this.logger.LogInformation(message);
    }
}