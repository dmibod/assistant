namespace Assistant.Market.Core.Services;

public interface IFeedService
{
    Task FeedAsync(TimeSpan lag);
}