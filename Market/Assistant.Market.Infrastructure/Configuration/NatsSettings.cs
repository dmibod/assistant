namespace Assistant.Market.Infrastructure.Configuration;

public class NatsSettings
{
    public string Url { get; set; } = null!;
        
    public string User { get; set; } = null!;

    public string Password { get; set; } = null!;
    
    public string AddStockRequestTopic { get; set; } = null!;

    public string RefreshStockRequestTopic { get; set; } = null!;
    
    public string CleanDataRequestTopic { get; set; } = null!;
    
    public string PublishMarketDataTopic { get; set; } = null!;
}