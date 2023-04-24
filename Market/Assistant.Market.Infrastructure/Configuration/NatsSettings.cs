namespace Assistant.Market.Infrastructure.Configuration;

public class NatsSettings : Common.Infrastructure.Configuration.NatsSettings
{
    public string AddStockRequestTopic { get; set; } = null!;

    public string RefreshStockRequestTopic { get; set; } = null!;
    
    public string CleanDataRequestTopic { get; set; } = null!;
    
    public string PublishMarketDataTopic { get; set; } = null!;
}