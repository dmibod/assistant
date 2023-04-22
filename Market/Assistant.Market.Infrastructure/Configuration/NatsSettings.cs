namespace Assistant.Market.Infrastructure.Configuration;

public class NatsSettings
{
    public string Url { get; set; } = null!;
        
    public string User { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FeedStockRequestTopic { get; set; } = null!;
}