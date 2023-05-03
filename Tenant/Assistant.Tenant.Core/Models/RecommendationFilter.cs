namespace Assistant.Tenant.Core.Models;

public class RecommendationFilter
{
    public int? MinAnnualPercent { get; set; }
    
    public decimal? MinPremium { get; set; }
    
    public int? MaxDte { get; set; }

    public bool? Otm { get; set; }

    public string AsDescription()
    {
        var filters = new List<string>();
            
        if (this.MinAnnualPercent.HasValue)
        {
            filters.Add($"annual roi >= {MinAnnualPercent}%");
        }
        if (this.MinPremium.HasValue)
        {
            filters.Add($"premium >= {MinPremium}$");
        }
        if (this.MaxDte.HasValue)
        {
            filters.Add($"dte <= {MaxDte}");
        }
        if (this.Otm.HasValue)
        {
            filters.Add(Otm.Value ? "otm" : "itm");
        }

        return filters.Count == 0 ? string.Empty : filters.Aggregate((curr, el) => $"{curr}, {el}");
    }
}