namespace Assistant.Market.Api.Controllers;

using System.Security.Claims;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Messaging.Models;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Security;
using Common.Core.Services;
using Common.Infrastructure.Security;
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
    /// Gets option chain by the stock ticker
    /// </summary>
    /// <param name="ticker"></param>
    [HttpGet("Stocks/{ticker}/Options")]
    public async Task<ActionResult> GetOptionChainAsync(string ticker)
    {
        var chain = await this.optionService.FindAsync(ticker);

        return this.Ok(chain);
    }

    /// <summary>
    /// Gets options change by the stock ticker
    /// </summary>
    /// <param name="ticker"></param>
    [HttpGet("Stocks/{ticker}/OptionsChange")]
    public async Task<ActionResult> GetOptionChangeAsync(string ticker)
    {
        var chain = await this.optionService.FindChangeAsync(ticker);

        return this.Ok(chain);
    }

    /// <summary>
    /// Adds stock to the system
    /// </summary>
    [HttpPost("Stocks/{ticker}"), Authorize("publishing")]
    public Task AddStockAsync(string ticker)
    {
        return this.busService.PublishAsync(this.stockCreateTopic, new TextMessage
        {
            Text = ticker
        });
    }

    /// <summary>
    /// Refresh stock data
    /// </summary>
    [HttpPut("Stocks/{ticker}"), Authorize("publishing")]
    public Task RefreshStockAsync(string ticker)
    {
        return this.busService.PublishAsync(this.stockRefreshTopic, ticker);
    }

    /// <summary>
    /// Publish market data
    /// </summary>
    [HttpPost("Publish"), Authorize("publishing")]
    public Task PublishAsync()
    {
        return this.busService.PublishAsync(this.dataPublishTopic);
    }
}