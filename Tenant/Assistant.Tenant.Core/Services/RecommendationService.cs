namespace Assistant.Tenant.Core.Services;

using System.Text.Json;
using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Common.Core.Utils;
using Helper.Core.Domain;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class RecommendationService : IRecommendationService
{
    private const int MaxRecsCount = 50;
    
    private readonly ITenantService tenantService;
    private readonly ITenantRepository repository;
    private readonly IWatchListService watchListService;
    private readonly IPositionService positionService;
    private readonly IMarketDataService marketDataService;
    private readonly ILogger<RecommendationService> logger;

    public RecommendationService(
        ITenantService tenantService,
        ITenantRepository repository,
        IWatchListService watchListService, 
        IPositionService positionService,
        IMarketDataService marketDataService,
        ILogger<RecommendationService> logger)
    {
        this.tenantService = tenantService;
        this.repository = repository;
        this.watchListService = watchListService;
        this.positionService = positionService;
        this.marketDataService = marketDataService;
        this.logger = logger;
    }

    public async Task<string?> FindSellPutsBoardId()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindSellPutsBoardId));

        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindSellPutsBoardIdAsync(tenant);
    }

    public async Task UpdateSellPutsBoardId(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellPutsBoardId), boardId);

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.UpdateSellPutsBoardIdAsync(tenant, boardId);
    }

    public async Task<SellPutsFilter?> GetSellPutsFilterAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.GetSellPutsFilterAsync));

        var tenant = await this.tenantService.EnsureExistsAsync();

        var filter = await this.repository.FindSellPutsFilterAsync(tenant);

        try
        {
            return string.IsNullOrEmpty(filter) ? null : JsonSerializer.Deserialize<SellPutsFilter>(filter);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task UpdateSellPutsFilterAsync(SellPutsFilter filter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellPutsFilterAsync), filter.AsDescription());

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.UpdateSellPutsFilterAsync(tenant, JsonSerializer.Serialize(filter));
    }

    public async Task<IEnumerable<SellOperation>> SellPutsAsync(SellPutsFilter filter, Func<int, ProgressTracker> trackerCreator)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SellPutsAsync), filter.AsDescription());

        var items = await this.watchListService.FindAllAsync();
        
        var tracker = trackerCreator(items.Count());

        var operations = Enumerable.Empty<SellOperation>();

        foreach (var item in items)
        {
            operations = operations.Union(await this.SellPutsAsync(item, filter, operations.Count()));

            tracker.Increase();
        }

        tracker.Finish();
        
        return operations;
    }

    public async Task<string?> FindSellCallsBoardId()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindSellCallsBoardId));

        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindSellCallsBoardIdAsync(tenant);
    }

    public async Task UpdateSellCallsBoardId(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellCallsBoardId), boardId);

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.UpdateSellCallsBoardIdAsync(tenant, boardId);
    }

    public async Task<SellCallsFilter?> GetSellCallsFilterAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.GetSellCallsFilterAsync));

        var tenant = await this.tenantService.EnsureExistsAsync();

        var filter = await this.repository.FindSellCallsFilterAsync(tenant);

        try
        {
            return string.IsNullOrEmpty(filter) ? null : JsonSerializer.Deserialize<SellCallsFilter>(filter);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task UpdateSellCallsFilterAsync(SellCallsFilter filter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellCallsFilterAsync), filter.AsDescription());

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.UpdateSellCallsFilterAsync(tenant, JsonSerializer.Serialize(filter));
    }

    public async Task<IEnumerable<SellOperation>> SellCallsAsync(SellCallsFilter filter, Func<int, ProgressTracker> trackerCreator)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SellCallsAsync), filter.AsDescription());

        var items = await this.watchListService.FindAllAsync();

        if (filter.Covered)
        {
            var positions = await this.positionService.FindAllAsync();

            var stocks = positions
                .Where(position => position is { Type: AssetType.Stock, Quantity: >= 100 })
                .Select(position => position.Ticker)
                .Distinct()
                .ToHashSet();

            items = items.Where(item => stocks.Contains(item.Ticker));
        }

        var tracker = trackerCreator(items.Count());

        var operations = Enumerable.Empty<SellOperation>();

        foreach (var item in items)
        {
            operations = operations.Union(await this.SellCallsAsync(item, filter, operations.Count()));

            tracker.Increase();
        }

        tracker.Finish();
        
        return operations;
    }

    public async Task<string?> FindOpenInterestBoardId()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindOpenInterestBoardId));

        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindOpenInterestBoardIdAsync(tenant);
    }

    public async Task UpdateOpenInterestBoardId(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateOpenInterestBoardId), boardId);

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.UpdateOpenInterestBoardIdAsync(tenant, boardId);
    }

    public async Task<OpenInterestFilter?> GetOpenInterestFilterAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.GetOpenInterestFilterAsync));

        var tenant = await this.tenantService.EnsureExistsAsync();

        var filter = await this.repository.FindOpenInterestFilterAsync(tenant);

        try
        {
            return string.IsNullOrEmpty(filter) ? null : JsonSerializer.Deserialize<OpenInterestFilter>(filter);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
            return null;
        }
    }

    public async Task UpdateOpenInterestFilterAsync(OpenInterestFilter filter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateOpenInterestFilterAsync), filter.AsDescription());

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.UpdateOpenInterestFilterAsync(tenant, JsonSerializer.Serialize(filter));
    }

    public async Task<IEnumerable<OptionAssetPrice>> OpenInterestAsync(OpenInterestFilter filter, Func<int, ProgressTracker> trackerCreator)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.OpenInterestAsync), filter.AsDescription());
        
        var items = await this.watchListService.FindAllAsync();

        var tracker = trackerCreator(items.Count());

        var options = Enumerable.Empty<OptionAssetPrice>();

        foreach (var item in items)
        {
            options = options.Union(await this.OpenInterestAsync(item, filter, options.Count()));

            tracker.Increase();
        }

        tracker.Finish();
        
        return options;

    }

    private async Task<IEnumerable<SellOperation>> SellCallsAsync(WatchListItem item, SellCallsFilter callsFilter, int opsCount)
    {
        if (opsCount > MaxRecsCount)
        {
            this.logger.LogWarning("Max recs count of {Maximum} has been reached", MaxRecsCount);
            return Enumerable.Empty<SellOperation>();
        }

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

            foreach (var price in optionPrices.Where(p => OptionUtils.GetSide(p.Ticker) == "C"))
            {
                var call = StockOption.Call(stock, OptionUtils.GetStrike(price.Ticker),
                    Expiration.FromYYYYMMDD(expiration));

                var op = call.Sell(price.Bid ?? decimal.Zero);
                if (op.BreakEvenStockPrice >= item.SellPrice && AreConditionsMet(op, callsFilter))
                {
                    sellOperations.Add(op);
                    if (opsCount + sellOperations.Count > MaxRecsCount)
                    {
                        this.logger.LogWarning("Max recs count of {Maximum} has been reached", MaxRecsCount);
                        return sellOperations;
                    }
                }
            }
        }

        return sellOperations;
    }
    
    private async Task<IEnumerable<SellOperation>> SellPutsAsync(WatchListItem item, SellPutsFilter putsFilter, int opsCount)
    {
        if (opsCount > MaxRecsCount)
        {
            this.logger.LogWarning("Max recs count of {Maximum} has been reached", MaxRecsCount);
            return Enumerable.Empty<SellOperation>();
        }

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
                if (op.BreakEvenStockPrice <= item.BuyPrice && AreConditionsMet(op, putsFilter))
                {
                    sellOperations.Add(op);
                    if (opsCount + sellOperations.Count > MaxRecsCount)
                    {
                        this.logger.LogWarning("Max recs count of {Maximum} has been reached", MaxRecsCount);
                        return sellOperations;
                    }
                }
            }
        }

        return sellOperations;
    }

   private async Task<IEnumerable<OptionAssetPrice>> OpenInterestAsync(WatchListItem item, OpenInterestFilter filter, int count)
    {
        if (count > MaxRecsCount)
        {
            this.logger.LogWarning("Max recs count of {Maximum} has been reached", MaxRecsCount);
            return Enumerable.Empty<OptionAssetPrice>();
        }

        var expirations = await this.marketDataService.FindExpirationsAsync(item.Ticker);
        if (expirations == null)
        {
            return Array.Empty<OptionAssetPrice>();
        }

        var options = new List<OptionAssetPrice>();

        foreach (var expiration in expirations)
        {
            var optionPrices = await this.marketDataService.FindOptionPricesAsync(item.Ticker, expiration);
            if (optionPrices == null) continue;

            var optionPricesChange = await this.marketDataService.FindOptionPricesChangeAsync(item.Ticker, expiration);
            if (optionPricesChange == null) continue;

            var changes = optionPricesChange.ToDictionary(o => o.Ticker);

            foreach (var price in optionPrices)
            {
                if (changes.TryGetValue(price.Ticker, out var change) && AreConditionsMet(price, change, filter))
                {
                    price.Last = change.OI;
                    
                    options.Add(price);
                    
                    if (count + options.Count > MaxRecsCount)
                    {
                        this.logger.LogWarning("Max recs count of {Maximum} has been reached", MaxRecsCount);
                        return options;
                    }
                }
            }
        }

        return options;
    }

   private static bool AreConditionsMet(OptionAssetPrice option, OptionAssetPrice change, OpenInterestFilter filter)
   {
       if (filter.MinContractsChange.HasValue)
       {
           if (!change.OI.HasValue || Math.Abs(change.OI.Value) < filter.MinContractsChange.Value)
           {
               return false;
           }
       }
       
       if (filter.MinPercentageChange.HasValue)
       {
           if (!option.OI.HasValue || !change.OI.HasValue)
           {
               return false;
           }

           if (option.OI.Value > decimal.Zero && Math.Abs(CalculationUtils.Percent(change.OI.Value / option.OI.Value)) < filter.MinPercentageChange.Value)
           {
               return false;
           }
       }

       return true;
   }

   private static bool AreConditionsMet(SellOperation op, RecommendationFilter filter)
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