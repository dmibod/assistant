namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;

public interface ISuggestionService
{
    Task<IEnumerable<SellOperation>> SellPutsAsync(SuggestionFilter filter, Func<int, ProgressTracker> trackerCreator);
}