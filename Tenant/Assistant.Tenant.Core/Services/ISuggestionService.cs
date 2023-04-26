namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Helper.Core.Domain;

public interface ISuggestionService
{
    Task<IEnumerable<SellOperation>> SuggestPutsAsync(SuggestionFilter filter);

    Task<IEnumerable<SellOperation>> SuggestPutsAsync(WatchListItem item, SuggestionFilter filter);
}