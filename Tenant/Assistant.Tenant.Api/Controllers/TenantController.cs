﻿namespace Assistant.Tenant.Api.Controllers;

using System.Security.Claims;
using System.Text.RegularExpressions;
using Assistant.Tenant.Core.Messaging;
using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Security;
using Common.Core.Services;
using Common.Core.Utils;
using Common.Infrastructure.Security;
using Helper.Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TenantController : ControllerBase
{
    private readonly IPositionService positionService;
    private readonly IPositionPublishingService positionPublishingService;
    private readonly IWatchListService watchListService;
    private readonly IPublishingService publishingService;
    private readonly ITenantService tenantService;
    private readonly IIdentityProvider identityProvider;
    private readonly IMarketDataService marketDataService;
    private readonly IBusService busService;
    private readonly string testMessageTopic;

    public TenantController(
        IPositionService positionService,
        IPositionPublishingService positionPublishingService,
        IWatchListService watchListService,
        IPublishingService publishingService,
        ITenantService tenantService,
        IIdentityProvider identityProvider,
        IMarketDataService marketDataService,
        ITopicResolver topicResolver,
        IBusService busService)
    {
        this.positionService = positionService;
        this.positionPublishingService = positionPublishingService;
        this.watchListService = watchListService;
        this.publishingService = publishingService;
        this.tenantService = tenantService;
        this.identityProvider = identityProvider;
        this.marketDataService = marketDataService;
        this.busService = busService;
        this.testMessageTopic = topicResolver.ResolveConfig(nameof(NatsSettings.PositionRemoveTopic));
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
    /// A list of stocks you watch, suggestions are produced based on buy/sell prices
    /// </summary>
    [HttpGet("WatchList")]
    public async Task<ActionResult> GetWatchListAsync()
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
    public Task<WatchListItem?> GetWatchListItemAsync(string ticker)
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
    public Task<WatchListItem> AddWatchListItemAsync(string ticker, decimal buyPrice, decimal sellPrice)
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
    public async Task<ActionResult> AddWatchListItemsAsync(string tickers, decimal buyPricePercent,
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
    public Task SetWatchListItemBuyPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetBuyPriceAsync(StockUtils.Format(ticker), price);
    }

    /// <summary>
    /// Here you can change sell price for the stock in your watch list
    /// </summary>
    [HttpPut("WatchList/{ticker}/SellPrice")]
    public Task SetWatchListItemSellPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetSellPriceAsync(StockUtils.Format(ticker), price);
    }

    /// <summary>
    /// Here you can remove stock from watch list
    /// </summary>
    [HttpDelete("WatchList/{ticker}")]
    public Task RemoveWatchListItemAsync(string ticker)
    {
        return this.watchListService.RemoveAsync(StockUtils.Format(ticker));
    }

    /// <summary>
    /// The list of your positions, ordered by 'Account' then 'Ticker' 
    /// </summary>
    [HttpGet("Positions")]
    public async Task<ActionResult> GetPositionsAsync()
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
    /// The list of your positions filtered by stock ticker, ordered by 'Account' then 'Ticker' 
    /// </summary>
    [HttpGet("Positions/{ticker}")]
    public async Task<ActionResult> GetPositionsByTickerAsync(string ticker)
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
    public async Task<ActionResult> GetPositionsByCriteriaAsync(string? account, string? ticker, string? tag,
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
            if (searchAccount && comparison(p.Account, account))
            {
                return true;
            }

            if (searchTag && comparison(p.Tag, tag))
            {
                return true;
            }

            if (searchTicker)
            {
                var positionTicker = p.Type == AssetType.Stock
                    ? p.Ticker
                    : OptionUtils.GetStock(p.Ticker);

                if (comparison(positionTicker, ticker))
                {
                    return true;
                }
            }

            return false;
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
    public Task<Position> AddStockPositionAsync(string account, string ticker, decimal averageCost, int size,
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
    public Task<Position> AddCallOptionPositionAsync(string account, string ticker, string yyyymmdd, decimal strike,
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
    public Task<Position> AddPutOptionPositionAsync(string account, string ticker, string yyyymmdd, decimal strike,
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
    /// Allows to modify account position quantity and average cost
    /// </summary>
    [HttpPut("Positions/{account}/{ticker}")]
    public Task TagPositionAsync(string account, string ticker, int quantity, decimal averageCost)
    {
        ticker = ticker.Trim().ToUpper();
        ticker = OptionUtils.IsValid(ticker) ? OptionUtils.Format(ticker) : StockUtils.Format(ticker);
        return this.positionService.UpdateAsync(account.Trim().ToUpper(), ticker, quantity, averageCost);
    }

    /// <summary>
    /// Allows to tag/untag account position
    /// </summary>
    [HttpPut("Positions/{account}/{ticker}/Tag")]
    public Task TagPositionAsync(string account, string ticker, string? tag)
    {
        ticker = ticker.Trim().ToUpper();
        ticker = OptionUtils.IsValid(ticker) ? OptionUtils.Format(ticker) : StockUtils.Format(ticker);
        return this.positionService.UpdateTagAsync(account.Trim().ToUpper(), ticker, tag ?? string.Empty);
    }

    /// <summary>
    /// Allows to rename tag
    /// </summary>
    [HttpPut("Positions/Tag/{tag}")]
    public Task ReplaceTagAsync(string tag, string? newTag)
    {
        return this.positionService.ReplaceTagAsync(tag, newTag ?? string.Empty);
    }

    /// <summary>
    /// Untags all positions 
    /// </summary>
    [HttpDelete("Positions/Tag")]
    public Task ResetTagsAsync()
    {
        return this.positionService.ResetTagAsync();
    }

    /// <summary>
    /// Searches for the untagged option positions and makes combos for those having the same underlying and expiration
    /// </summary>
    [HttpPost("Positions/Tag/Options")]
    public Task AutoTagOptionsAsync()
    {
        return this.positionService.AutoTagOptionsAsync();
    }

    /// <summary>
    /// Here you can remove account position
    /// </summary>
    [HttpDelete("Positions/{account}/{ticker}")]
    public Task RemovePositionAsync(string account, string ticker)
    {
        ticker = ticker.Trim().ToUpper();
        ticker = OptionUtils.IsValid(ticker) ? OptionUtils.Format(ticker) : StockUtils.Format(ticker);
        return this.positionService.RemoveAsync(account.Trim().ToUpper(), ticker, false);
    }

    /// <summary>
    /// Publishes your positions
    /// </summary>
    [HttpPost("Positions/Publish")]
    public Task PublishPositionsAsync()
    {
        return this.positionPublishingService.PublishAsync();
    }

    /// <summary>
    /// Default filter for the recommendations
    /// </summary>
    /// <returns></returns>
    [HttpGet("Recommendations/Filter")]
    public Task<RecommendationFilter?> GetRecommendationFilterAsync()
    {
        return this.tenantService.GetDefaultFilterAsync();
    }

    /// <summary>
    /// Allows to specify default filter for the recommendations
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="maxDte">Max days till expiration, days</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    [HttpPost("Recommendations/Filter")]
    public async Task UpdateRecommendationFilterAsync(int? minAnnualPercent, decimal? minPremium, int? maxDte,
        bool? otm)
    {
        var filter = new RecommendationFilter
        {
            MinAnnualPercent = minAnnualPercent,
            MinPremium = minPremium,
            MaxDte = maxDte,
            Otm = otm
        };

        await this.tenantService.UpdateDefaultFilterAsync(filter);
    }

    /// <summary>
    /// Generates 'Sell Calls' recommendations (call options you need to pay attention to) based on your watch list, sell prices and filtering criteria 
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="maxDte">Max days till expiration, days</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    /// <param name="considerPositions">true - to check if there is stock position available for 'covered' calls</param>
    [HttpPost("Recommendations/SellCalls")]
    public async Task PublishSellCallsAsync(int? minAnnualPercent, decimal? minPremium, int? maxDte, bool? otm, bool considerPositions)
    {
        var defaultFilter = await this.tenantService.GetDefaultFilterAsync();

        var filter = defaultFilter ?? new RecommendationFilter();

        if (minAnnualPercent.HasValue)
        {
            filter.MinAnnualPercent = minAnnualPercent;
        }

        if (minPremium.HasValue)
        {
            filter.MinPremium = minPremium;
        }

        if (maxDte.HasValue)
        {
            filter.MaxDte = maxDte;
        }

        if (otm.HasValue)
        {
            filter.Otm = otm;
        }

        await this.publishingService.PublishSellCallsAsync(filter, considerPositions);
    }

    /// <summary>
    /// Generates 'Sell Puts' recommendations (put options you need to pay attention to) based on your watch list, buy prices and filtering criteria 
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="maxDte">Max days till expiration, days</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    [HttpPost("Recommendations/SellPuts")]
    public async Task PublishSellPutsAsync(int? minAnnualPercent, decimal? minPremium, int? maxDte, bool? otm)
    {
        var defaultFilter = await this.tenantService.GetDefaultFilterAsync();

        var filter = defaultFilter ?? new RecommendationFilter();

        if (minAnnualPercent.HasValue)
        {
            filter.MinAnnualPercent = minAnnualPercent;
        }

        if (minPremium.HasValue)
        {
            filter.MinPremium = minPremium;
        }

        if (maxDte.HasValue)
        {
            filter.MaxDte = maxDte;
        }

        if (otm.HasValue)
        {
            filter.Otm = otm;
        }

        await this.publishingService.PublishSellPutsAsync(filter);
    }

    [HttpPost("Test"), Authorize("administration")]
    public Task TestAsync(string account, string ticker)
    {
        ticker = ticker.Trim().ToUpper();
        ticker = OptionUtils.IsValid(ticker) ? OptionUtils.Format(ticker) : StockUtils.Format(ticker);
        return this.busService.PublishAsync(this.testMessageTopic, new PositionRemoveMessage
        {
            Tenant = this.identityProvider.Identity.Name!,
            Account = account.Trim().ToUpper(),
            Ticker = ticker
        });
    }
}