namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Helper.Core.Domain;

public interface IPublishingService
{
    Task PublishPositionsAsync();

    Task PublishSuggestionsAsync(IEnumerable<SellOperation> operations, SuggestionFilter filter);
}