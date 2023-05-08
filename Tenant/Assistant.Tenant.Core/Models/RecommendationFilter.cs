namespace Assistant.Tenant.Core.Models;

public abstract class RecommendationFilter
{
    public int? MinAnnualPercent { get; set; }
    
    public decimal? MinPremium { get; set; }
    
    public int? MaxDte { get; set; }

    public bool? Otm { get; set; }

    public virtual string AsDescription()
    {
        var filters = new List<string>();
            
        if (this.MinAnnualPercent.HasValue)
        {
            filters.Add($"annual roi >= {this.MinAnnualPercent}%");
        }
        
        if (this.MinPremium.HasValue)
        {
            filters.Add($"premium >= {this.MinPremium}$");
        }
        
        if (this.MaxDte.HasValue)
        {
            filters.Add($"dte <= {this.MaxDte}");
        }
        
        if (this.Otm.HasValue)
        {
            filters.Add(this.Otm.Value ? "otm" : "itm");
        }

        return filters.Count == 0 ? string.Empty : filters.Aggregate((curr, el) => $"{curr}, {el}");
    }
}