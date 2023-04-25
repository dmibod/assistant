namespace Common.Core.Utils;

public class ProgressTracker
{
    private readonly Action<int> progress;
    private readonly int increasePercent;
    private readonly int totalItems;
    private int currentItems;

    public ProgressTracker(int totalItems, int increasePercent, Action<int> progress)
    {
        this.totalItems = totalItems;
        this.increasePercent = increasePercent;
        this.progress = progress;
        this.Reset();
    }

    public void Increase()
    {
        this.currentItems++;

        var progressPercents = this.currentItems * 100 / this.totalItems;
        progressPercents = progressPercents > 100 ? 100 : progressPercents;

        if (progressPercents == 100 || progressPercents % this.increasePercent == 0)
        {
            this.progress(progressPercents);
        }
    }

    public void Increase(int value)
    {
        while (value-- > 0) this.Increase();
    }

    public void Reset()
    {
        this.currentItems = 0;
    }
}