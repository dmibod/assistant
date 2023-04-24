namespace Common.Infrastructure.Services;

using Microsoft.Extensions.Hosting;

public abstract class BaseHostedService : IHostedService
{
    private readonly Lazy<string> nameOf;

    protected BaseHostedService()
    {
        this.nameOf = new Lazy<string>(() => this.GetType().Name);
    }

    protected string ServiceName => this.nameOf.Value;
    
    protected abstract void LogMessage(string message);
    
    protected abstract void LogError(string error);
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.LogMessage($"{this.ServiceName} is starting...");

        await this.OnStartAsync();

        this.LogMessage($"{this.ServiceName} has started.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.LogMessage($"{this.ServiceName} is stopping...");

        await this.OnStopAsync();

        this.LogMessage($"{this.ServiceName} has stopped.");
    }

    protected abstract Task OnStartAsync();
    
    protected abstract Task OnStopAsync();
}