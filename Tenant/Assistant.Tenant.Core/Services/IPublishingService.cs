namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IPublishingService
{
    Task PublishPositionsAsync();

    Task PublishSuggestionsAsync(IEnumerable<object> operations, SuggestionFilter appliedFilter);
}