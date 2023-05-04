namespace Common.Infrastructure.Services;

using Microsoft.Extensions.Hosting;

public abstract class BaseHostedService : BackgroundService
{
    private readonly Lazy<string> nameOf;

    protected BaseHostedService()
    {
        this.nameOf = new Lazy<string>(() => this.GetType().Name);
    }

    protected string ServiceName => this.nameOf.Value;
    
    protected abstract void LogMessage(string message);
    
    protected abstract void LogError(string error);
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.LogMessage($"{this.ServiceName} is starting...");

        await base.StartAsync(cancellationToken);

        this.LogMessage($"{this.ServiceName} has started.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.LogMessage($"{this.ServiceName} is stopping...");

        await base.StopAsync(cancellationToken);

        this.LogMessage($"{this.ServiceName} has stopped.");
    }
}