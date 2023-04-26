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

    [HttpPost("Token")]
    [EnableCors("CorsPolicy")]
    public async Task<ActionResult> Token()
    {
        var identity = this.identityProvider.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return this.BadRequest();
        }

        var tenant = await this.tenantService.GetOrCreateAsync();

        var result = new
        {
            User = tenant.Name,
            Expiration = identity.GetExpiration().ToLongDateString()
        };

        return this.Ok(result);
    }

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

    [HttpPut("WatchList/{ticker}/BuyPrice")]
    public Task SetWatchListItemBuyPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetBuyPriceAsync(ticker.ToUpper(), price);
    }

    [HttpPut("WatchList/{ticker}/SellPrice")]
    public Task SetWatchListItemSellPriceAsync(string ticker, decimal price)
    {
        return this.watchListService.SetSellPriceAsync(ticker.ToUpper(), price);
    }

    [HttpDelete("WatchList")]
    public Task RemoveWatchListItemAsync(string ticker)
    {
        return this.watchListService.RemoveAsync(ticker.ToUpper());
    }

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

    [HttpPost("Positions/{account}/{ticker}/Call/{yyyymmmdd}")]
    public Task<Position> AddCallOptionPositionAsync(string account, string ticker, string yyyymmmdd, decimal strike, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Ticker = OptionUtils.OptionTicker(ticker.ToUpper(), yyyymmmdd, strike, true),
            Type = AssetType.Option,
            AverageCost = averageCost,
            Quantity = size,
            Tag = tag
        };

        return this.positionService.CreateAsync(position);
    }

    [HttpPost("Positions/{account}/{ticker}/Put/{yyyymmmdd}")]
    public Task<Position> AddPutOptionPositionAsync(string account, string ticker, string yyyymmmdd, decimal strike, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Ticker = OptionUtils.OptionTicker(ticker.ToUpper(), yyyymmmdd, strike, false),
            Type = AssetType.Option,
            AverageCost = averageCost,
            Quantity = size,
            Tag = tag
        };

        return this.positionService.CreateAsync(position);
    }

    [HttpPut("Positions/{account}/{ticker}/{tag}")]
    public Task TagPositionAsync(string account, string ticker, string tag)
    {
        return this.positionService.UpdateTagAsync(account.ToUpper(), ticker.ToUpper(), tag);
    }

    [HttpDelete("Positions/{account}")]
    public Task RemovePositionAsync(string account, string ticker)
    {
        return this.positionService.RemoveAsync(account.ToUpper(), ticker.ToUpper());
    }

    [HttpPost("Positions/Publish")]
    public Task PublishPositions()
    {
        return this.publishingService.PublishPositionsAsync();
    }
    
    [HttpPost("Suggestions/Publish")]
    public async Task PublishSuggestions(int? minAnnualPercent, decimal? minPremium, int? maxDte, bool? Otm)
    {
        var filter = new SuggestionFilter
        {
            MinAnnualPercent = minAnnualPercent,
            MinPremium = minPremium,
            MaxDte = maxDte,
            Otm = Otm
        };

        var operations = await this.suggestionService.SuggestPutsAsync(filter);
        
        await this.publishingService.PublishSuggestionsAsync(operations, filter);
    }
}