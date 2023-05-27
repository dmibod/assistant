namespace Assistant.Market.Api.Controllers;

using System.Security.Claims;
using Assistant.Market.Core.Messaging;
using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Security;
using Common.Core.Services;
using Common.Infrastructure.Security;
using Helper.Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class MarketController : ControllerBase
{
    private readonly IStockService stockService;
    private readonly IOptionService optionService;
    private readonly IBusService busService;
    private readonly IIdentityProvider identityProvider;
    private readonly string dataPublishTopic;
    private readonly string stockCreateTopic;
    private readonly string stockRemoveTopic;
    private readonly string stockRefreshTopic;

    public MarketController(
        IStockService stockService,
        IOptionService optionService,
        IIdentityProvider identityProvider,
        IBusService busService,
        ITopicResolver topicResolver)
    {
        this.stockService = stockService;
        this.optionService = optionService;
        this.identityProvider = identityProvider;
        this.busService = busService;
        this.dataPublishTopic = topicResolver.ResolveConfig(nameof(NatsSettings.DataPublishTopic));
        this.stockCreateTopic = topicResolver.ResolveConfig(nameof(NatsSettings.StockCreateTopic));
        this.stockRemoveTopic = topicResolver.ResolveConfig(nameof(NatsSettings.StockRemoveTopic));
        this.stockRefreshTopic = topicResolver.ResolveConfig(nameof(NatsSettings.StockRefreshTopic));
    }

    /// <summary>
    /// Allows to get 'User' and 'Expiration' from your token
    /// </summary>
    [HttpPost("Token")]
    public ActionResult Token()
    {
        var identity = this.identityProvider.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return this.BadRequest();
        }

        var result = new
        {
            User = identity.Name,
            Expiration = identity.GetExpiration().ToLongDateString()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// Gets a list of stocks tracked by the system, ordered by 'last refresh' time
    /// </summary>
    [HttpGet("Stocks")]
    public async Task<ActionResult> GetStocksAsync()
    {
        var stocks = await this.stockService.FindAllAsync();

        var result = new
        {
            Count = stocks.Count(),
            Items = stocks.OrderByDescending(stock => stock.LastRefresh).ToArray()
        };

        return this.Ok(result);
    }

    /// <summary>
    /// Get stock by ticker
    /// </summary>
    /// <param name="ticker">Stock ticker</param>
    [HttpGet("Stocks/{ticker}")]
    public Task<Stock?> GetStockAsync(string ticker)
    {
        return this.stockService.FindByTickerAsync(StockUtils.Format(ticker));
    }

    /// <summary>
    /// Gets option chain by the stock ticker
    /// </summary>
    /// <param name="ticker">Stock ticker</param>
    [HttpGet("Stocks/{ticker}/Options")]
    public async Task<ActionResult> GetOptionChainAsync(string ticker)
    {
        var chain = await this.optionService.FindAsync(StockUtils.Format(ticker));

        return this.Ok(chain);
    }

    /// <summary>
    /// Gets option expiration data
    /// </summary>
    /// <param name="ticker">Stock ticker</param>
    /// <param name="expiration">Expiration in format YYYYMMDD</param>
    /// <returns></returns>
    [HttpGet("Stocks/{ticker}/{expiration}/Options")]
    public Task<OptionExpiration?> GetOptionExpirationAsync(string ticker, string expiration)
    {
        return this.optionService.FindExpirationAsync(StockUtils.Format(ticker), expiration);
    }

    /// <summary>
    /// Gets options change by the stock ticker
    /// </summary>
    /// <param name="ticker">Stock ticker</param>
    /// <param name="minOpenInterest">Minimum open interest</param>
    [HttpGet("Stocks/{ticker}/OptionsChange")]
    public async Task<ActionResult> GetOptionChangeAsync(string ticker, decimal? minOpenInterest)
    {
        var chain = await this.optionService.FindChangeAsync(StockUtils.Format(ticker));

        if (minOpenInterest.HasValue)
        {
            Func<OptionContracts, bool> filter = contracts =>
            {
                if (contracts.Call != null && Math.Abs(contracts.Call.OI) >= minOpenInterest.Value)
                {
                    return true;
                }

                if (contracts.Put != null && Math.Abs(contracts.Put.OI) >= minOpenInterest.Value)
                {
                    return true;
                }

                return false;
            };
            
            chain.Expirations.Values.ToList().ForEach(expiration =>
            {
                expiration.Contracts = expiration.Contracts
                    .Where(pair => filter(pair.Value))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            });
            
            chain.Expirations = chain.Expirations
                .Where(pair => pair.Value.Contracts.Count > 0)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        return this.Ok(chain);
    }

    /// <summary>
    /// Adds stock to the system
    /// </summary>
    [HttpPost("Stocks/{ticker}"), Authorize("publishing")]
    public Task StockAddAsync(string ticker)
    {
        return this.busService.PublishAsync(this.stockCreateTopic, new StockCreateMessage
        {
            Ticker = StockUtils.Format(ticker)
        });
    }

    /// <summary>
    /// Remove stock from the system
    /// </summary>
    [HttpDelete("Stocks/{ticker}"), Authorize("publishing")]
    public Task StockRemoveAsync(string ticker)
    {
        return this.busService.PublishAsync(this.stockRemoveTopic, new StockRemoveMessage
        {
            Ticker = StockUtils.Format(ticker)
        });
    }

    /// <summary>
    /// Refresh stock data
    /// </summary>
    [HttpPut("Stocks/{ticker}"), Authorize("publishing")]
    public Task RefreshStockAsync(string ticker)
    {
        return this.busService.PublishAsync(this.stockRefreshTopic, new StockRefreshMessage
        {
            Ticker = StockUtils.Format(ticker)
        });
    }

    /// <summary>
    /// Publish market data
    /// </summary>
    [HttpPost("Publish"), Authorize("publishing")]
    public Task PublishAsync(bool marketData = true, bool openInterest = true)
    {
        return this.busService.PublishAsync(this.dataPublishTopic, new DataPublishMessage
        {
            MarketData = marketData,
            OpenInterest = openInterest
        });
    }
}