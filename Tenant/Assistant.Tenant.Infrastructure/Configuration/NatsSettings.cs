namespace Assistant.Tenant.Infrastructure.Configuration;

public class NatsSettings : Common.Infrastructure.Configuration.NatsSettings
{
    public string StockCreateTopic { get; set; } = null!;
    
    public string PositionCreateTopic { get; set; } = null!;
    
    public string PositionRefreshTopic { get; set; } = null!;
    
    public string PositionRemoveTopic { get; set; } = null!;
    
    public string WatchListRefreshTopic { get; set; } = null!;
}