namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Microsoft.Extensions.Logging;

public class WatchListPublishingService : IWatchListPublishingService
{
    private const string WatchList = "Watch List";
    
    private readonly IWatchListService watchListService;
    private readonly IMarketDataService marketDataService;
    private readonly IKanbanService kanbanService;
    private readonly ILogger<WatchListPublishingService> logger;

    public WatchListPublishingService(
        IWatchListService watchListService,
        IMarketDataService marketDataService,
        IKanbanService kanbanService,
        ILogger<WatchListPublishingService> logger)
    {
        this.watchListService = watchListService;
        this.marketDataService = marketDataService;
        this.kanbanService = kanbanService;
        this.logger = logger;
    }

    public async Task PublishAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishAsync));

        var board = await this.GetBoardAsync();

        try
        {
            await this.PublishAsync(board);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }
        finally
        {
            await this.watchListService.UpdateKanbanBoardId(board.Id);
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }
    
    private async Task<Board> GetBoardAsync()
    {
        var now = DateTime.UtcNow;
        var name = $"{WatchList} {now.ToShortDateString()} {now.ToShortTimeString()}";
        
        var boardId = await this.watchListService.FindKanbanBoardId();
        if (!string.IsNullOrEmpty(boardId))
        {
            await this.watchListService.UpdateKanbanBoardId(string.Empty);
            var board = await this.kanbanService.FindBoardAsync(boardId);
            if (board != null)
            {
                board.Name = name;
                await this.kanbanService.UpdateBoardAsync(board);
                return board;
            }
        }

        return await this.kanbanService.CreateBoardAsync(new Board { Name = name });
    }

    private async Task PublishAsync(Board board)
    {
        const int maxDescTickers = 90;

        var watchList = (await this.watchListService.FindAllAsync()).OrderBy(item => item.Ticker).ToList();
        var tickers = watchList.Select(item => item.Ticker).Distinct().ToHashSet();
        var stocks = (await this.marketDataService.FindStockPricesAsync(tickers)).ToDictionary(stock => stock.Ticker);

        var tracker = new ProgressTracker(watchList.Count, 1,
            progress =>
            {
                this.kanbanService.SetBoardProgressStateAsync(board.Id, progress).GetAwaiter().GetResult();
            });

        var lanes = await this.kanbanService.FindLanesAsync(board.Id);

        var lane = await this.GetOrCreateLaneAsync(board, WatchList, "companies ordered by ticker", lanes);

        var allCards = await this.kanbanService.FindCardsAsync(board.Id, lane.Id);

        var actualCards = new List<Card>();

        foreach (var item in watchList)
        {
            var description = this.ItemToContent(item, stocks[item.Ticker]);

            var card = await this.GetOrCreateCardAsync(board, lane, item.Ticker, description, allCards);
            
            actualCards.Add(card);

            await this.watchListService.UpdateKanbanCardIdAsync(item.Ticker, card.Id);

            tracker.Increase();
        }

        await this.RemoveObsoleteCardsAsync(board, lane, allCards, actualCards);

        tracker.Finish();

        board.Description = tickers.Count > 0 
            ? tickers.Take(maxDescTickers).Aggregate((curr, i) => $"{curr}, {i}") 
            : string.Empty;

        if (tickers.Count > maxDescTickers)
        {
            board.Description += "...";
        }

        await this.kanbanService.UpdateBoardAsync(board);
    }
    
    private async Task<Lane> GetOrCreateLaneAsync(Board board, string name, string description, IEnumerable<Lane> lanes)
    {
        var lane = lanes.FirstOrDefault(lane => lane.Name == name && lane.Description == description);
        if (lane != null)
        {
            if (lane.Description != description)
            {
                await this.kanbanService.UpdateLaneAsync(board.Id, lane.Id, description);
            }

            return lane;
        }

        return await this.kanbanService.CreateCardLaneAsync(board.Id, board.Id,
            new Lane { Name = name, Description = description });
    }
    
    private async Task<Card> GetOrCreateCardAsync(Board board, Lane lane, string name, string description,
        IEnumerable<Card> cards)
    {
        var card = cards.FirstOrDefault(card => card.Name == name);
        if (card != null)
        {
            if (card.Description != description)
            {
                await this.kanbanService.UpdateCardAsync(board.Id, card.Id, description);
            }

            return card;
        }

        return await this.kanbanService.CreateCardAsync(board.Id, lane.Id,
            new Card { Name = name, Description = description });
    }
    
    private async Task RemoveObsoleteCardsAsync(Board board, Lane lane, IEnumerable<Card> allCards, IEnumerable<Card> actualCards)
    {
        foreach (var cardId in allCards.Select(card => card.Id).Except(actualCards.Select(card => card.Id)))
        {
            await this.kanbanService.RemoveCardAsync(board.Id, lane.Id, cardId);
        }
    }
    
    private string ItemToContent(WatchListItem item, AssetPrice price)
    {
        return "["
               + RenderUtils.PairToContent(RenderUtils.PropToContent("Buy"), RenderUtils.PropToContent(FormatUtils.FormatPrice(item.BuyPrice), IsGreater(item.BuyPrice, price.Last) ? RenderUtils.RedStyle : RenderUtils.NoStyle)) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("Sell"), RenderUtils.PropToContent(FormatUtils.FormatPrice(item.SellPrice), IsGreater(price.Last, item.SellPrice) ? RenderUtils.RedStyle : RenderUtils.NoStyle)) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("Price"), RenderUtils.PropToContent(FormatUtils.FormatPrice(price.Last))) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("Cap"), RenderUtils.PropToContent(FormatUtils.FormatCap(price.MarketCap), IsGreater(price.MarketCap, 1000_000_000) ? RenderUtils.NoStyle : RenderUtils.RedStyle)) +
               "]";
    }

    private static bool IsGreater(double? left, double? right)
    {
        return (left ?? 0) > (right ?? 0);
    }

    private static bool IsGreater(decimal? left, decimal? right)
    {
        return (left ?? decimal.Zero) > (right ?? decimal.Zero);
    }
}