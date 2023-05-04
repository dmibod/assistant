namespace Common.Infrastructure.Services;

public abstract class BaseTimerService : BaseHostedService, IDisposable
{
    private readonly TimeSpan interval;
    private readonly TimeSpan initialDelay;
    private Timer? timer;

    protected BaseTimerService(TimeSpan interval, TimeSpan initialDelay)
    {
        this.interval = interval;
        this.initialDelay = initialDelay;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.timer = new Timer(this.TryDoWork, null, this.initialDelay, this.interval);

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        this.timer?.Change(Timeout.Infinite, 0);

        return base.StopAsync(cancellationToken);
    }

    private void TryDoWork(object? state)
    {
        try
        {
            this.LogMessage($"{this.ServiceName} is working...");

            this.DoWork(state);
        }
        catch (Exception e)
        {
            this.LogError(e.Message);
        }
    }

    protected abstract void DoWork(object? state);

    protected static int GetInitialDelay(int from, int to)
    {
        var seed = DateTime.Now.Ticks % int.MaxValue;
        var random = new Random((int)seed).Next(to - from);
        return from + random;
    }

    public override void Dispose()
    {
        this.timer?.Dispose();
        
        base.Dispose();
    }
}