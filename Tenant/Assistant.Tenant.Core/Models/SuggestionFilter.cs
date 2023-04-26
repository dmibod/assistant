namespace Assistant.Tenant.Core.Models;

public class SuggestionFilter
{
    public int? MinAnnualPercent { get; set; }
    
    public decimal? MinPremium { get; set; }
    
    public int? MaxDte { get; set; }

    public bool? Otm { get; set; }

    public string AsDescription()
    {
        var filters = new List<string>();
            
        if (MinAnnualPercent.HasValue)
        {
            filters.Add($"annual roi >= {MinAnnualPercent}%");
        }
        if (MinPremium.HasValue)
        {
            filters.Add($"premium >= {MinPremium}$");
        }
        if (MaxDte.HasValue)
        {
            filters.Add($"dte <= {MaxDte}");
        }
        if (Otm.HasValue)
        {
            filters.Add(Otm.Value ? "otm" : "itm");
        }

        return filters.Aggregate((curr, el) => $"{curr}, {el}");
    }
}