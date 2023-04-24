namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public class PublishingService : IPublishingService
{
    public Task PublishPositions()
    {
        throw new NotImplementedException();
    }

    public Task PublishSuggestions(IEnumerable<object> operations, SuggestionFilter appliedFilter)
    {
        throw new NotImplementedException();
    }
}