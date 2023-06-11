namespace Assistant.Market.Core.Models;

public class OpenInterestFilter
{
    public int Top { get; set; }

    public decimal MinPercent { get; set; }

    public bool PublishIncrease { get; set; }

    public bool PublishDecrease { get; set; }

    public string AsDescription()
    {
        var filters = new List<string>();

        if (this.PublishIncrease)
        {
            filters.Add($"increase");
        }

        if (this.PublishDecrease)
        {
            filters.Add($"decrease");
        }

        filters.Add($"top {this.Top} changes");
        
        filters.Add($"change >= {this.MinPercent}% of max change");

        return filters.Count == 0 ? string.Empty : filters.Aggregate((curr, el) => $"{curr}, {el}");
    }
}