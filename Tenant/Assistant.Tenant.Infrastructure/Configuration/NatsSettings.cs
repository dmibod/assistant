namespace Assistant.Tenant.Infrastructure.Configuration;

public class NatsSettings : Common.Infrastructure.Configuration.NatsSettings
{
    public string AddStockRequestTopic { get; set; } = null!;

    public string PublishSuggestionsTopic { get; set; } = null!;
}