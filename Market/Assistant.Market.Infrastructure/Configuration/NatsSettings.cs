namespace Assistant.Market.Infrastructure.Configuration;

public class NatsSettings : Common.Infrastructure.Configuration.NatsSettings
{
    public string StockCreateTopic { get; set; } = null!;

    public string StockRefreshTopic { get; set; } = null!;
    
    public string DataCleanTopic { get; set; } = null!;
    
    public string DataPublishTopic { get; set; } = null!;
}