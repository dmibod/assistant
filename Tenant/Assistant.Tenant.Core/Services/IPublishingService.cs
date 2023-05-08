namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IPublishingService
{
    Task PublishSellPutsAsync(SellPutsFilter filter);
    
    Task PublishSellCallsAsync(SellCallsFilter filter);
}