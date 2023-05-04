namespace Common.Infrastructure.Services;

using NATS.Client;

public abstract class BaseWorkerService : BaseHostedService
{
    private readonly IConnection connection;
    private readonly string topic;
    private IAsyncSubscription subscription;

    protected BaseWorkerService(string topic, IConnection connection)
    {
        this.connection = connection;
        this.topic = topic;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.subscription = this.connection.SubscribeAsync(this.topic, this.TryDoWork);
        
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.subscription.Unsubscribe();
        await this.subscription.DrainAsync();

        await base.StopAsync(cancellationToken);
    }

    private void TryDoWork(object? sender, MsgHandlerEventArgs args)
    {
        try
        {
            this.LogMessage($"{this.ServiceName} is working...");

            this.DoWork(sender, args);
        }
        catch (Exception e)
        {
            this.LogError(e.Message);
        }
    }

    protected abstract void DoWork(object? sender, MsgHandlerEventArgs args);
}