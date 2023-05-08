namespace Assistant.Tenant.Core.Models;

public class SellCallsFilter : RecommendationFilter
{
    public bool Covered { get; set; }

    public override string AsDescription()
    {
        var filters = new List<string>();

        var description = base.AsDescription();

        if (!string.IsNullOrWhiteSpace(description))
        {
            filters.Add(description);
        }

        if (this.Covered)
        {
            filters.Add("covered");
        }

        return filters.Count == 0 ? string.Empty : filters.Aggregate((curr, el) => $"{curr}, {el}");;
    }
}