namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IPublishingService
{
    Task PublishPositions();

    Task PublishSuggestions(IEnumerable<object> operations, SuggestionFilter appliedFilter);
}