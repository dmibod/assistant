namespace Assistant.Market.Api.Services;

public abstract class BaseTimerService : IHostedService, IDisposable
{
    private readonly TimeSpan interval;
    private readonly TimeSpan initialDelay;
    private Timer? timer;

    private readonly Lazy<string> nameOf;
    protected string ServiceName => this.nameOf.Value;

    protected BaseTimerService(TimeSpan interval, TimeSpan initialDelay)
    {
        this.interval = interval;
        this.initialDelay = initialDelay;
        this.nameOf = new Lazy<string>(() => this.GetType().Name);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.LogMessage($"{this.ServiceName} is starting...");

        this.timer = new Timer(this.DoWork, null, this.initialDelay, this.interval);

        this.LogMessage($"{this.ServiceName} has started.");

        return Task.CompletedTask;
    }

    protected abstract void DoWork(object? state);

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.LogMessage($"{this.ServiceName} is stopping...");

        this.timer?.Change(Timeout.Infinite, 0);

        this.LogMessage($"{this.ServiceName} has stopped.");

        return Task.CompletedTask;
    }

    protected abstract void LogMessage(string message);

    protected static int GetInitialDelay(int from, int to)
    {
        var seed = DateTime.Now.Ticks % int.MaxValue;
        var random = new Random((int)seed).Next(to - from);
        return from + random;
    }

    public virtual void Dispose()
    {
        this.timer?.Dispose();
    }
}