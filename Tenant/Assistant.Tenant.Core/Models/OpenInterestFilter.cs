namespace Assistant.Tenant.Core.Models;

public class OpenInterestFilter : RecommendationFilter
{
    public decimal? MinContractsChange { get; set; }
    
    public decimal? MinPercentageChange { get; set; }
    
    public override string AsDescription()
    {
        var filters = new List<string>();

        var description = base.AsDescription();

        if (!string.IsNullOrWhiteSpace(description))
        {
            filters.Add(description);
        }
            
        if (this.MinContractsChange.HasValue)
        {
            filters.Add($"min contracts change >= {this.MinContractsChange}");
        }
        
        if (this.MinPercentageChange.HasValue)
        {
            filters.Add($"min % change >= {this.MinPercentageChange}");
        }

        return filters.Count == 0 ? string.Empty : filters.Aggregate((curr, el) => $"{curr}, {el}");
    }
}