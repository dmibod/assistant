namespace Assistant.Tenant.Api.Controllers;

using System.Security.Claims;
using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Services;
using Common.Core.Security;
using Common.Core.Utils;
using Common.Infrastructure.Security;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TenantController : ControllerBase
{
    private readonly IPositionService positionService;
    private readonly IWatchListService watchListService;
    private readonly ISuggestionService suggestionService;
    private readonly IPublishingService publishingService;
    private readonly ITenantService tenantService;
    private readonly IIdentityProvider identityProvider;

    public TenantController(IPositionService positionService, IWatchListService watchListService, ISuggestionService suggestionService, IPublishingService publishingService, ITenantService tenantService, IIdentityProvider identityProvider)
    {
        this.positionService = positionService;
        this.watchListService = watchListService;
        this.suggestionService = suggestionService;
        this.publishingService = publishingService;
        this.tenantService = tenantService;
        this.identityProvider = identityProvider;
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
            Ticker = ticker.ToUpper(),
            BuyPrice = buyPrice,
            SellPrice = sellPrice
        };

        return this.watchListService.CreateAsync(item);
    }

    /// <summary>
    /// Here you can change buy price for the stock in your watch list
    /// </summary>
    [HttpPut("WatchList/{ticker}/BuyPrice")]
    public Task SetWatchListItemBuyPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetBuyPriceAsync(ticker.ToUpper(), price);
    }

    /// <summary>
    /// Here you can change sell price for the stock in your watch list
    /// </summary>
    [HttpPut("WatchList/{ticker}/SellPrice")]
    public Task SetWatchListItemSellPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetSellPriceAsync(ticker.ToUpper(), price);
    }

    /// <summary>
    /// Here you can remove stock from watch list
    /// </summary>
    [HttpDelete("WatchList/{ticker}")]
    public Task RemoveWatchListItemAsync(string ticker)
    {
        return this.watchListService.RemoveAsync(ticker.ToUpper());
    }

    /// <summary>
    /// The list of your positions, ordered by 'Account' then 'Ticker' 
    /// </summary>
    [HttpGet("Positions")]
    public async Task<ActionResult> GetPositionsAsync()
    {
        var positions = await this.positionService.FindAllAsync();

        var result = new
        {
            Count = positions.Count(),
            Items = positions.OrderBy(position => position.Account).ThenBy(position => position.Ticker).ToArray()
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
    public Task<Position> AddStockPositionAsync(string account, string ticker, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Ticker = ticker.ToUpper(),
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
    public Task<Position> AddCallOptionPositionAsync(string account, string ticker, string yyyymmdd, decimal strike, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Ticker = OptionUtils.OptionTicker(ticker.ToUpper(), yyyymmdd, strike, true),
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
    public Task<Position> AddPutOptionPositionAsync(string account, string ticker, string yyyymmdd, decimal strike, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Ticker = OptionUtils.OptionTicker(ticker.ToUpper(), yyyymmdd, strike, false),
            Type = AssetType.Option,
            AverageCost = averageCost,
            Quantity = size,
            Tag = tag
        };

        return this.positionService.CreateAsync(position);
    }

    /// <summary>
    /// Allows to tag/untag account position
    /// </summary>
    [HttpPut("Positions/{account}/{ticker}")]
    public Task TagPositionAsync(string account, string ticker, string? tag)
    {
        return this.positionService.UpdateTagAsync(account.ToUpper(), ticker.ToUpper(), tag ?? string.Empty);
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
        return this.positionService.RemoveAsync(account.ToUpper(), ticker.ToUpper());
    }

    /// <summary>
    /// Publishes your positions
    /// </summary>
    [HttpPost("Positions/Publish")]
    public Task PublishPositions()
    {
        return this.publishingService.PublishPositionsAsync();
    }

    /// <summary>
    /// Default filter for the suggestions generation
    /// </summary>
    /// <returns></returns>
    [HttpGet("Suggestions/Filter")]
    public Task<SuggestionFilter?> GetDefaultFilterAsync()
    {
        return this.tenantService.GetDefaultFilterAsync();
    }

    /// <summary>
    /// Allows to specify default filter for the suggestions generation
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="maxDte">Max days till expiration, days</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    [HttpPost("Suggestions/Filter")]
    public async Task UpdateDefaultFilter(int? minAnnualPercent, decimal? minPremium, int? maxDte, bool? otm)
    {
        var filter = new SuggestionFilter
        {
            MinAnnualPercent = minAnnualPercent,
            MinPremium = minPremium,
            MaxDte = maxDte,
            Otm = otm
        };

        await this.tenantService.UpdateDefaultFilterAsync(filter);
    }

    /// <summary>
    /// Generates suggestions (call/put options you need to pay attention to) based on your watch list, buy/sell prices and filtering criteria 
    /// </summary>
    /// <param name="minAnnualPercent">Min annual yield, %</param>
    /// <param name="minPremium">Min premium (option contract price), $</param>
    /// <param name="maxDte">Max days till expiration, days</param>
    /// <param name="otm">true - out of the money options, false - in the money options</param>
    [HttpPost("Suggestions/Publish")]
    public async Task PublishSuggestions(int? minAnnualPercent, decimal? minPremium, int? maxDte, bool? otm)
    {
        var defaultFilter = await this.tenantService.GetDefaultFilterAsync();

        var filter = defaultFilter ?? new SuggestionFilter();
        
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

        await this.publishingService.PublishSuggestionsAsync(filter);
    }
}