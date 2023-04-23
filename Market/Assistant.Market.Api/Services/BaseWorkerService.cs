namespace Assistant.Market.Api.Services;

using NATS.Client;

public abstract class BaseWorkerService : BaseHostedService, IDisposable
{
    private readonly IConnection connection;
    private readonly string topic;
    private IAsyncSubscription subscription;

    protected BaseWorkerService(IConnection connection, string topic)
    {
        this.connection = connection;
        this.topic = topic;
    }

    protected override Task OnStartAsync()
    {
        this.subscription = this.connection.SubscribeAsync(this.topic, this.TryDoWork);
        
        return Task.CompletedTask;
    }

    protected override async Task OnStopAsync()
    {
        this.subscription.Unsubscribe();
        await this.subscription.DrainAsync();
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

    public virtual void Dispose()
    {
    }
}