namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class SuggestionService : ISuggestionService
{
    private readonly IWatchListService watchListService;
    private readonly IMarketDataService marketDataService;
    private readonly ILogger<SuggestionService> logger;

    public SuggestionService(IWatchListService watchListService, IMarketDataService marketDataService,
        ILogger<SuggestionService> logger)
    {
        this.watchListService = watchListService;
        this.marketDataService = marketDataService;
        this.logger = logger;
    }

    public async Task<IEnumerable<SellOperation>> SuggestPutsAsync(SuggestionFilter filter, Func<int, ProgressTracker> trackerCreator)
    {
        this.logger.LogInformation("{Method}", nameof(this.SuggestPutsAsync));

        var items = await this.watchListService.FindAllAsync();

        var tracker = trackerCreator(items.Count());

        var operations = Enumerable.Empty<SellOperation>();

        foreach (var item in items)
        {
            operations = operations.Union(await this.SuggestPutsAsync(item, filter));

            tracker.Increase();
        }

        tracker.Finish();
        
        return operations;
    }

    public async Task<IEnumerable<SellOperation>> SuggestPutsAsync(WatchListItem item, SuggestionFilter filter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SuggestPutsAsync), item.Ticker);
        
        var expirations = await this.marketDataService.FindExpirationsAsync(item.Ticker);
        if (expirations == null)
        {
            return Array.Empty<SellOperation>();
        }

        var stockPrices = await this.marketDataService.FindStockPricesAsync(new HashSet<string>(new[] { item.Ticker }));

        var sellOperations = new List<SellOperation>();
        foreach (var expiration in expirations)
        {
            var optionPrices = await this.marketDataService.FindOptionPricesAsync(item.Ticker, expiration);
            if (optionPrices == null) continue;

            var stock = Stock.From(item.Ticker, expirations.Select(exp => Expiration.FromYYYYMMDD(exp)).AsQueryable());

            var stockPrice = stockPrices.FirstOrDefault(s => s.Ticker == item.Ticker);
            if (stockPrice != null)
            {
                stock.Price = MarketPrice.From(stockPrice.Last ?? decimal.Zero);
            }

            foreach (var price in optionPrices.Where(p => OptionUtils.GetSide(p.Ticker) == "P"))
            {
                var put = StockOption.Put(stock, OptionUtils.GetStrike(price.Ticker),
                    Expiration.FromYYYYMMDD(expiration));

                var op = put.Sell(price.Bid ?? decimal.Zero);
                if (op.BreakEvenStockPrice <= item.BuyPrice && AreConditionsMet(op, filter))
                {
                    sellOperations.Add(op);
                }
            }
        }

        return sellOperations;
    }

    private static bool AreConditionsMet(SellOperation op, SuggestionFilter filter)
    {
        if (filter.MinAnnualPercent.HasValue)
        {
            if (CalculationUtils.Percent(op.AnnualRoi) < filter.MinAnnualPercent.Value)
            {
                return false;
            }
        }

        if (filter.MinPremium.HasValue)
        {
            if (op.ContractPrice < filter.MinPremium.Value)
            {
                return false;
            }
        }

        if (filter.MaxDte.HasValue)
        {
            if (op.Option.DaysTillExpiration > filter.MaxDte.Value)
            {
                return false;
            }
        }

        if (filter.Otm.HasValue)
        {
            if (filter.Otm.Value)
            {
                return !op.Option.InTheMoney;
            }
            
            return op.Option.InTheMoney;
        }

        return true;
    }
}