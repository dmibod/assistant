namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface ISuggestionService
{
    // returns sell operations
    IEnumerable<object> SuggestPuts(WatchItem asset, SuggestionFilter filter);
}