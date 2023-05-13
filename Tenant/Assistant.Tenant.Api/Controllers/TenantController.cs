namespace Assistant.Tenant.Api.Controllers;

using System.Security.Claims;
using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Services;
using Common.Core.Security;
using Common.Infrastructure.Security;
using Helper.Core.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TenantController : ControllerBase
{
    private readonly IPositionService positionService;
    private readonly IPositionPublishingService positionPublishingService;
    private readonly IWatchListService watchListService;
    private readonly IWatchListPublishingService watchListPublishingService;
    private readonly IPublishingService publishingService;
    private readonly IRecommendationService recommendationService;
    private readonly ITenantService tenantService;
    private readonly IIdentityProvider identityProvider;
    private readonly IMarketDataService marketDataService;

    public TenantController(
        IPositionService positionService,
        IPositionPublishingService positionPublishingService,
        IWatchListService watchListService,
        IWatchListPublishingService watchListPublishingService,
        IPublishingService publishingService,
        IRecommendationService recommendationService,
        ITenantService tenantService,
        IIdentityProvider identityProvider,
        IMarketDataService marketDataService)
    {
        this.positionService = positionService;
        this.positionPublishingService = positionPublishingService;
        this.watchListService = watchListService;
        this.watchListPublishingService = watchListPublishingService;
        this.publishingService = publishingService;
        this.recommendationService = recommendationService;
        this.tenantService = tenantService;
        this.identityProvider = identityProvider;
        this.marketDataService = marketDataService;
    }

    /// <summary>
    /// Allows to get 'User' and 'Expiration' from your token
    /// </summary>
    [HttpPost("Token")]
    [EnableCors("CorsPolicy")]
    public async Task<ActionResult> Token()
    {
        var identity = this.identityProvider.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return this.BadRequest();
        }

        var tenant = await this.tenantService.EnsureExistsAsync();

        var result = new
        {
            User = tenant,
            Expiration = identity.GetExpiration().ToLongDateString()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// A list of stocks you watch, sell option recommendations are based on buy/sell prices
    /// </summary>
    [HttpGet("WatchList")]
    public async Task<ActionResult> WatchListGetAsync()
    {
        var watchList = await this.watchListService.FindAllAsync();

        var result = new
        {
            Count = watchList.Count(),
            Items = watchList.OrderBy(item => item.Ticker).ToArray()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// Gets single watch list item by stock ticker
    /// </summary>
    [HttpGet("WatchList/{ticker}")]
    public Task<WatchListItem?> WatchListGetItemAsync(string ticker)
    {
        return this.watchListService.FindByTickerAsync(StockUtils.Format(ticker));
    }

    /// <summary>
    /// Adds stock to your watch list, market data for the stock will be fetched and continuously refreshed
    /// </summary>
    /// <param name="ticker">Stock ticker, ex. AAPL, TSLA etc...</param>
    /// <param name="buyPrice">The price at which you are comfortable to own the stock</param>
    /// <param name="sellPrice">The price at which you are willing to get rid of the stock</param>
    [HttpPost("WatchList/{ticker}")]
    public Task<WatchListItem> WatchListAddItemAsync(string ticker, decimal buyPrice, decimal sellPrice)
    {
        var item = new WatchListItem
        {
            Ticker = StockUtils.Format(ticker),
            BuyPrice = buyPrice,
            SellPrice = sellPrice
        };

        return this.watchListService.CreateOrUpdateAsync(item, false);
    }

    /// <summary>
    /// Adds multiple stocks to your watch list
    /// </summary>
    /// <param name="tickers">Multiple stock tickers separate by comma, ex. AAPL, TSLA, AMZN</param>
    /// <param name="buyPricePercent">The % amount below current price, for ex. stock price 100$ in case buyPricePercent is 10%, buy price is 90$</param>
    /// <param name="sellPricePercent">The % amount above current price, for ex. stock price 100$ in case sellPricePercent is 10%, sell price is 110$</param>
    [HttpPost("WatchList/AddMultiple")]
    public async Task<ActionResult> WatchListAddItemsAsync(string tickers, decimal buyPricePercent,
        decimal sellPricePercent)
    {
        var validatedTickers = tickers
            .Split(",")
            .Select(StockUtils.Format)
            .Distinct()
            .ToHashSet();

        if (validatedTickers.Count == 0)
        {
            return this.Ok(new
            {
                Count = 0,
                Items = Array.Empty<WatchListItem>()
            });
        }

        var prices =
            (await this.marketDataService.FindStockPricesAsync(validatedTickers)).ToDictionary(item => item.Ticker);

        var buyPriceFactor = (100.0m - buyPricePercent) / 100.0m;
        var sellPriceFactor = (100.0m + sellPricePercent) / 100.0m;

        var list = new List<WatchListItem>();

        foreach (var ticker in validatedTickers)
        {
            var last = prices[ticker].Last;
            var item = new WatchListItem
            {
                Ticker = ticker,
                BuyPrice = prices.ContainsKey(ticker) && last.HasValue
                    ? Math.Round(last.Value * buyPriceFactor)
                    : decimal.Zero,
                SellPrice = prices.ContainsKey(ticker) && last.HasValue
                    ? Math.Round(last.Value * sellPriceFactor)
                    : decimal.Zero
            };

            item = await this.watchListService.CreateOrUpdateAsync(item, true);

            list.Add(item);
        }

        return this.Ok(new
        {
            list.Count,
            Items = list.ToArray()
        });
    }

    /// <summary>
    /// Here you can change buy price for the stock in your watch list
    /// </summary>
    [HttpPut("WatchList/{ticker}/BuyPrice")]
    public Task WatchListSetItemBuyPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetBuyPriceAsync(StockUtils.Format(ticker), price);
    }

    /// <summary>
    /// Here you can change sell price for the stock in your watch list
    /// </summary>
    [HttpPut("WatchList/{ticker}/SellPrice")]
    public Task WatchListSetItemSellPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetSellPriceAsync(StockUtils.Format(ticker), price);
    }

    /// <summary>
    /// Here you can remove stock from watch list
    /// </summary>
    [HttpDelete("WatchList/{ticker}")]
    public Task WatchListRemoveItemAsync(string ticker)
    {
        return this.watchListService.RemoveAsync(StockUtils.Format(ticker), false);
    }

    /// <summary>
    /// Publishes your watch list
    /// </summary>
    [HttpPost("WatchList/Publish")]
    public Task WatchListPublishAsync()
    {
        return this.watchListPublishingService.PublishAsync();
    }

    /// <summary>
    /// The list of your positions, ordered by 'Account' then 'Ticker' 
    /// </summary>
    [HttpGet("Positions")]
    public async Task<ActionResult> PositionsGetAsync()
    {
        var positions = await this.positionService.FindAllAsync();

        var list = positions.ToList();

        var result = new
        {
            list.Count,
            Items = list.OrderBy(position => position.Account).ThenBy(position => position.Ticker).ToArray()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// The list of your accounts 
    /// </summary>
    [HttpGet("Positions/Accounts")]
    public async Task<ActionResult> PositionsGetAccountsAsync()
    {
        var positions = await this.positionService.FindAllAsync();

        var list = positions.GroupBy(p => p.Account).Select(g => $"{g.Key} ({g.Count()})").OrderBy(a => a).ToList();

        var result = new
        {
            list.Count,
            Items = list.ToArray()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// The list of your positions filtered by stock ticker, ordered by 'Account' then 'Ticker' 
    /// </summary>
    [HttpGet("Positions/{ticker}")]
    public async Task<ActionResult> PositionsGetByTickerAsync(string ticker)
    {
        ticker = StockUtils.Format(ticker);

        var positions = await this.positionService.FindAllAsync();

        var filtered = positions.Where(p =>
            ticker == (p.Type == AssetType.Stock
                ? p.Ticker
                : OptionUtils.GetStock(p.Ticker))).ToList();

        var result = new
        {
            filtered.Count,
            Items = filtered.OrderBy(position => position.Account).ThenBy(position => position.Ticker).ToArray()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// The list of your positions filtered by account, stock ticker, tag, ordered by 'Account' then 'Ticker'
    /// </summary>
    /// <param name="contains">True - applies 'contains' for the criteria, false - 'does not contain', empty - 'exact match'.</param>
    /// <returns></returns>
    [HttpGet("Positions/Search")]
    public async Task<ActionResult> PositionsGetByCriteriaAsync(string? account, string? ticker, AssetType? type, string? tag,
        bool? contains)
    {
        var searchAccount = !string.IsNullOrWhiteSpace(account);
        if (searchAccount)
        {
            account = account.ToUpper();
        }

        var searchTicker = !string.IsNullOrWhiteSpace(ticker);
        if (searchTicker)
        {
            ticker = StockUtils.Format(ticker);
        }

        var searchType = type.HasValue;

        var searchTag = !string.IsNullOrWhiteSpace(tag);
        if (searchTag)
        {
            tag = tag.ToUpper();
        }

        Func<string?, string> safeStr = s => s ?? string.Empty;
        Func<string?, string?, bool> containsFn = (s1, s2) => safeStr(s1).Contains(safeStr(s2));
        Func<string?, string?, bool> doesNotContainFn = (s1, s2) => !containsFn(s1, s2);
        Func<string?, string?, bool> equalityFn = (s1, s2) => safeStr(s1) == safeStr(s2);

        var comparison = contains.HasValue
            ? contains.Value ? containsFn : doesNotContainFn
            : equalityFn;

        var positions = await this.positionService.FindAllAsync();

        var filtered = positions.Where(p =>
        {
            if (searchAccount && !comparison(p.Account, account))
            {
                return false;
            }

            if (searchType && p.Type != type.Value)
            {
                return false;
            }

            if (searchTag && !comparison(p.Tag, tag))
            {
                return false;
            }

            if (searchTicker)
            {
                var positionTicker = p.Type == AssetType.Stock
                    ? p.Ticker
                    : OptionUtils.GetStock(p.Ticker);

                if (!comparison(positionTicker, ticker))
                {
                    return false;
                }
            }

            return true;
        }).ToList();

        var result = new
        {
            filtered.Count,
            Items = filtered.OrderBy(position => position.Account).ThenBy(position => position.Ticker).ToArray()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// Here you can add stock position
    /// </summary>
    /// <param name="account">The account your position belongs to</param>
    /// <param name="ticker">Stock ticker</param>
    /// <param name="averageCost">The cost of 1 stock in your position (averaged value)</param>
    /// <param name="size">Positive value means number of stocks you own, negative value for the short position</param>
    /// <param name="tag">Use tags to group positions into 'combos'</param>
    /// <returns></returns>
    [HttpPost("Positions/{account}/{ticker}")]
    public Task<Position> PositionAddStockAsync(string account, string ticker, decimal averageCost, int size,
        string tag = "")
    {
        var position = new Position
        {
            Account = account.Trim().ToUpper(),
            Ticker = StockUtils.Format(ticker),
            Type = AssetType.Stock,
            AverageCost = averageCost,
            Quantity = size
        };

        return this.positionService.CreateAsync(position);
    }

    /// <summary>
    /// Here you can add call option position
    /// </summary>
    /// <param name="account">The account your position belongs to</param>
    /// <param name="ticker">The underlying stock ticker</param>
    /// <param name="yyyymmdd">Option expiration, ex. 20230428 (April 28, 2023)</param>
    /// <param name="strike">Option strike</param>
    /// <param name="averageCost">The cost of 1 option contract in your position (averaged value)</param>
    /// <param name="size">Positive value means number of option contracts you bought, negative value for the sold option contracts</param>
    /// <param name="tag">Use tags to group positions into 'combos'</param>
    /// <returns></returns>
    [HttpPost("Positions/{account}/{ticker}/Call/{yyyymmdd}")]
    public Task<Position> PositionAddCallOptionAsync(string account, string ticker, string yyyymmdd, decimal strike,
        decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.Trim().ToUpper(),
            Ticker = OptionUtils.OptionTicker(StockUtils.Format(ticker), yyyymmdd, strike, true),
            Type = AssetType.Option,
            AverageCost = averageCost,
            Quantity = size,
            Tag = tag
        };

        return this.positionService.CreateAsync(position);
    }

    /// <summary>
    /// Here you can add put option position
    /// </summary>
    /// <param name="account">The account your position belongs to</param>
    /// <param name="ticker">The underlying stock ticker</param>
    /// <param name="yyyymmdd">Option expiration, ex. 20230428 (April 28, 2023)</param>
    /// <param name="strike">Option strike</param>
    /// <param name="averageCost">The cost of 1 option contract in your position (averaged value)</param>
    /// <param name="size">Positive value means number of option contracts you bought, negative value for the sold option contracts</param>
    /// <param name="tag">Use tags to group positions into 'combos'</param>
    /// <returns></returns>
    [HttpPost("Positions/{account}/{ticker}/Put/{yyyymmdd}")]
    public Task<Position> PositionAddPutOptionAsync(string account, string ticker, string yyyymmdd, decimal strike,
        decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.Trim().ToUpper(),
            Ticker = OptionUtils.OptionTicker(StockUtils.Format(ticker), yyyymmdd, strike, false),
            Type = AssetType.Option,
            AverageCost = averageCost,
            Quantity = size,
            Tag = tag
        };

        return this.positionService.CreateAsync(position);
    }
    
    /// <summary>
    /// Here you can remove account position
    /// </summary>
    [HttpDelete("Positions/{account}/{ticker}")]
    public Task PositionRemoveAsync(string account, string ticker)
    {
        ticker = ticker.Trim().ToUpper();
        ticker = OptionUtils.IsValid(ticker) ? OptionUtils.Format(ticker) : StockUtils.Format(ticker);
        return this.positionService.RemoveAsync(account.Trim().ToUpper(), ticker, false);
    }

    /// <summary>
    /// Publishes your positions
    /// </summary>
    [HttpPost("Positions/Publish")]
    public Task PositionsPublishAsync()
    {
        return this.positionPublishingService.PublishAsync();
    }

    /// <summary>
    /// Allows to modify account position quantity and average cost
    /// </summary>
    [HttpPut("Positions/{account}/{ticker}")]
    public Task PositionTagAsync(string account, string ticker, int quantity, decimal averageCost)
    {
        ticker = ticker.Trim().ToUpper();
        ticker = OptionUtils.IsValid(ticker) ? OptionUtils.Format(ticker) : StockUtils.Format(ticker);
        return this.positionService.UpdateAsync(account.Trim().ToUpper(), ticker, quantity, averageCost);
    }

    /// <summary>
    /// Allows to tag/untag account position
    /// </summary>
    [HttpPut("Positions/{account}/{ticker}/Tag")]
    public Task PositionTagAsync(string account, string ticker, string? tag)
    {
        ticker = ticker.Trim().ToUpper();
        ticker = OptionUtils.IsValid(ticker) ? OptionUtils.Format(ticker) : StockUtils.Format(ticker);
        return this.positionService.UpdateTagAsync(account.Trim().ToUpper(), ticker, tag ?? string.Empty);
    }

    /// <summary>
    /// Allows to rename tag
    /// </summary>
    [HttpPut("Positions/Tag/{tag}")]
    public Task TagReplaceAsync(string tag, string? newTag)
    {
        return this.positionService.ReplaceTagAsync(tag, newTag ?? string.Empty);
    }

    /// <summary>
    /// Untags all positions 
    /// </summary>
    [HttpDelete("Positions/Tag")]
    public Task TagsResetAsync()
    {
        return this.positionService.ResetTagAsync();
    }

    /// <summary>
    /// Searches for the untagged option positions and makes combos for those having the same underlying and expiration
    /// </summary>
    [HttpPost("Positions/Tag/Options")]
    public Task TagOptionsAsync()
    {
        return this.positionService.AutoTagOptionsAsync();
    }
    
    /// <summary>
    /// Default filter for the sell calls recommendations
    /// </summary>
    /// <returns></returns>
    [HttpGet("Recommendations/SellCalls/Filter")]
    public Task<SellCallsFilter?> RecommendationGetSellCallsFilterAsync()
    {
        return this.recommendationService.GetSellCallsFilterAsync();
    }

    /// <summary>
    /// Allows to specify default filter for the sell calls recommendations
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="minDte">Min days till expiration</param>
    /// <param name="maxDte">Max days till expiration</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    /// <param name="monthly">true - use monthly option expirations only</param>
    /// <param name="covered">true - to check if there is stock position available for 'covered' calls</param>
    [HttpPost("Recommendations/SellCalls/Filter")]
    public async Task RecommendationUpdateSellCallsFilterAsync(int? minAnnualPercent, decimal? minPremium, int? minDte, int? maxDte,
        bool? otm, bool? monthly, bool covered)
    {
        var filter = new SellCallsFilter
        {
            MinAnnualPercent = minAnnualPercent,
            MinPremium = minPremium,
            MinDte = minDte,
            MaxDte = maxDte,
            Otm = otm,
            MonthlyExpirations = monthly,
            Covered = covered
        };

        await this.recommendationService.UpdateSellCallsFilterAsync(filter);
    }

    /// <summary>
    /// Generates 'Sell Calls' recommendations (call options you need to pay attention to) based on your watch list, sell prices and filtering criteria 
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="minDte">Min days till expiration</param>
    /// <param name="maxDte">Max days till expiration</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    /// <param name="monthly">true - use monthly option expirations only</param>
    /// <param name="covered">true - to check if there is stock position available for 'covered' calls</param>
    [HttpPost("Recommendations/SellCalls")]
    public async Task RecommendationSellCallsAsync(int? minAnnualPercent, decimal? minPremium, int? minDte, int? maxDte, bool? otm, bool? monthly, bool? covered)
    {
        var defaultFilter = await this.recommendationService.GetSellCallsFilterAsync();

        var filter = defaultFilter ?? new SellCallsFilter();

        if (minAnnualPercent.HasValue)
        {
            filter.MinAnnualPercent = minAnnualPercent;
        }

        if (minPremium.HasValue)
        {
            filter.MinPremium = minPremium;
        }

        if (minDte.HasValue)
        {
            filter.MinDte = minDte;
        }

        if (maxDte.HasValue)
        {
            filter.MaxDte = maxDte;
        }

        if (otm.HasValue)
        {
            filter.Otm = otm;
        }

        if (monthly.HasValue)
        {
            filter.MonthlyExpirations = monthly;
        }

        if (covered.HasValue)
        {
            filter.Covered = covered.Value;
        }

        await this.publishingService.PublishSellCallsAsync(filter);
    }

    /// <summary>
    /// Default filter for the sell puts recommendations
    /// </summary>
    /// <returns></returns>
    [HttpGet("Recommendations/SellPuts/Filter")]
    public Task<SellPutsFilter?> RecommendationGetSellPutsFilterAsync()
    {
        return this.recommendationService.GetSellPutsFilterAsync();
    }

    /// <summary>
    /// Allows to specify default filter for the sell puts recommendations
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="minDte">Min days till expiration</param>
    /// <param name="maxDte">Max days till expiration</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    /// <param name="monthly">true - use monthly option expirations only</param>
    [HttpPost("Recommendations/SellPuts/Filter")]
    public async Task RecommendationUpdateSellPutsFilterAsync(int? minAnnualPercent, decimal? minPremium, int? minDte, int? maxDte,
        bool? otm, bool? monthly)
    {
        var filter = new SellPutsFilter
        {
            MinAnnualPercent = minAnnualPercent,
            MinPremium = minPremium,
            MinDte = minDte,
            MaxDte = maxDte,
            Otm = otm,
            MonthlyExpirations = monthly
        };

        await this.recommendationService.UpdateSellPutsFilterAsync(filter);
    }

    /// <summary>
    /// Generates 'Sell Puts' recommendations (put options you need to pay attention to) based on your watch list, buy prices and filtering criteria 
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="minDte">Min days till expiration</param>
    /// <param name="maxDte">Max days till expiration</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    /// <param name="monthly">true - use monthly option expirations only</param>
    [HttpPost("Recommendations/SellPuts")]
    public async Task RecommendationSellPutsAsync(int? minAnnualPercent, decimal? minPremium, int? minDte, int? maxDte, bool? otm, bool? monthly)
    {
        var defaultFilter = await this.recommendationService.GetSellPutsFilterAsync();

        var filter = defaultFilter ?? new SellPutsFilter();

        if (minAnnualPercent.HasValue)
        {
            filter.MinAnnualPercent = minAnnualPercent;
        }

        if (minPremium.HasValue)
        {
            filter.MinPremium = minPremium;
        }

        if (minDte.HasValue)
        {
            filter.MinDte = minDte;
        }

        if (maxDte.HasValue)
        {
            filter.MaxDte = maxDte;
        }

        if (otm.HasValue)
        {
            filter.Otm = otm;
        }

        if (monthly.HasValue)
        {
            filter.MonthlyExpirations = monthly;
        }

        await this.publishingService.PublishSellPutsAsync(filter);
    }
    
    /// <summary>
    /// Default filter for the open interest recommendations
    /// </summary>
    /// <returns></returns>
    [HttpGet("Recommendations/OpenInterest/Filter")]
    public Task<OpenInterestFilter?> RecommendationGetOpenInterestFilterAsync()
    {
        return this.recommendationService.GetOpenInterestFilterAsync();
    }

    /// <summary>
    /// Allows to specify default filter for the open interest recommendations
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="minDte">Min days till expiration</param>
    /// <param name="maxDte">Max days till expiration</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    /// <param name="monthly">true - use monthly option expirations only</param>
    /// <param name="minContractsChange">min contracts change</param>
    /// <param name="minPercentageChange">min percentage change</param>
    [HttpPost("Recommendations/OpenInterest/Filter")]
    public async Task RecommendationUpdateOpenInterestFilterAsync(int? minAnnualPercent, decimal? minPremium, int? minDte, int? maxDte,
        bool? otm, bool? monthly, decimal? minContractsChange, decimal? minPercentageChange)
    {
        var filter = new OpenInterestFilter
        {
            MinAnnualPercent = minAnnualPercent,
            MinPremium = minPremium,
            MinDte = minDte,
            MaxDte = maxDte,
            Otm = otm,
            MonthlyExpirations = monthly,
            MinContractsChange = minContractsChange,
            MinPercentageChange = minPercentageChange
        };

        await this.recommendationService.UpdateOpenInterestFilterAsync(filter);
    }

    /// <summary>
    /// Generates 'Open Interest' recommendations (options you need to pay attention to) based on your watch list, open interest change and filtering criteria 
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="minDte">Min days till expiration</param>
    /// <param name="maxDte">Max days till expiration</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    /// <param name="monthly">true - use monthly option expirations only</param>
    /// <param name="minContractsChange">min contracts change</param>
    /// <param name="minPercentageChange">min percentage change</param>
    [HttpPost("Recommendations/OpenInterest")]
    public async Task RecommendationOpenInterestAsync(int? minAnnualPercent, decimal? minPremium, int? minDte, int? maxDte, bool? otm, bool? monthly, decimal? minContractsChange, decimal? minPercentageChange)
    {
        var defaultFilter = await this.recommendationService.GetOpenInterestFilterAsync();

        var filter = defaultFilter ?? new OpenInterestFilter();

        if (minAnnualPercent.HasValue)
        {
            filter.MinAnnualPercent = minAnnualPercent;
        }

        if (minPremium.HasValue)
        {
            filter.MinPremium = minPremium;
        }

        if (minDte.HasValue)
        {
            filter.MinDte = minDte;
        }

        if (maxDte.HasValue)
        {
            filter.MaxDte = maxDte;
        }

        if (otm.HasValue)
        {
            filter.Otm = otm;
        }

        if (monthly.HasValue)
        {
            filter.MonthlyExpirations = monthly;
        }

        if (minContractsChange.HasValue)
        {
            filter.MinContractsChange = minContractsChange;
        }
        
        if (minPercentageChange.HasValue)
        {
            filter.MinPercentageChange = minPercentageChange;
        }

        await this.publishingService.PublishOpenInterestAsync(filter);
    }
}