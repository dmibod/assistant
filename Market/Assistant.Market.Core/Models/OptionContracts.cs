namespace Assistant.Market.Core.Models;

public class OptionContracts
{
    public decimal Strike { get; set; }

    public OptionContract Call { get; set; }

    public OptionContract Put { get; set; }
}