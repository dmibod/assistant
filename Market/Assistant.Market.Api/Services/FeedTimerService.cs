﻿namespace Assistant.Market.Api.Services;

using NATS.Client;

public class FeedTimerService : IHostedService, IDisposable
{
    private readonly TimeSpan interval = TimeSpan.FromMinutes(1);
    private readonly IConnection connection;
    private readonly Msg feedRequest = new("feed.stock.request");
    private readonly ILogger<FeedTimerService> logger;
    private Timer? timer;

    public FeedTimerService(IConnection connection, ILogger<FeedTimerService> logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(FeedTimerService)} is starting...");

        this.timer = new Timer(DoWork, null, TimeSpan.FromSeconds(10), this.interval);

        this.logger.LogInformation($"{nameof(FeedTimerService)} has started.");

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        this.logger.LogInformation($"{nameof(FeedTimerService)} is working...");

        this.connection.Publish(feedRequest);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"{nameof(FeedTimerService)} is stopping...");

        this.timer?.Change(Timeout.Infinite, 0);

        this.logger.LogInformation($"{nameof(FeedTimerService)} has stopped.");

        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        this.timer?.Dispose();
    }
}