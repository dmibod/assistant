namespace Assistant.Tenant.Core.Models;

public abstract class RecommendationFilter
{
    public int? MinAnnualPercent { get; set; }
    
    public decimal? MinPremium { get; set; }

    public int? MinDte { get; set; }

    public int? MaxDte { get; set; }

    public bool? Otm { get; set; }

    public int? MinVolume { get; set; }

    public bool? MonthlyExpirations { get; set; }

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

        if (this.MinDte.HasValue)
        {
            filters.Add($"dte >= {this.MinDte}");
        }

        if (this.MaxDte.HasValue)
        {
            filters.Add($"dte <= {this.MaxDte}");
        }
        
        if (this.MinVolume.HasValue)
        {
            filters.Add($"vol >= {this.MinVolume}");
        }

        if (this.Otm.HasValue)
        {
            filters.Add(this.Otm.Value ? "otm" : "itm");
        }

        if (this.MonthlyExpirations.HasValue)
        {
            filters.Add(this.MonthlyExpirations.Value ? "exp(m)" : "exp(w)");
        }

        return filters.Count == 0 ? string.Empty : filters.Aggregate((curr, el) => $"{curr}, {el}");
    }
}