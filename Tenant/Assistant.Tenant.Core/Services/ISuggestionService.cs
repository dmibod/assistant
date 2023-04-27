namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;

public interface ISuggestionService
{
    Task<IEnumerable<SellOperation>> SuggestPutsAsync(SuggestionFilter filter, Func<int, ProgressTracker> trackerCreator);

    Task<IEnumerable<SellOperation>> SuggestPutsAsync(WatchListItem item, SuggestionFilter filter);
}