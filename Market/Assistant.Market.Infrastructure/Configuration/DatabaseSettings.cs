namespace Assistant.Market.Infrastructure.Configuration;

public class DatabaseSettings : Common.Infrastructure.Configuration.DatabaseSettings
{
    public string StockCollectionName { get; set; } = null!;
    
    public string OptionCollectionName { get; set; } = null!;
}