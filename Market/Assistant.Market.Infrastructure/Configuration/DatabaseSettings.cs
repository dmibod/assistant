namespace Assistant.Market.Infrastructure.Configuration;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string StockCollectionName { get; set; } = null!;
    
    public string OptionPriceCollectionName { get; set; } = null!;
}