namespace Assistant.Market.Api.Controllers;

using System.Security.Claims;
using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Security;
using Common.Core.Services;
using Common.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("[controller]")]
public class MarketController : ControllerBase
{
    private readonly IStockService stockService;
    private readonly IOptionService optionService;
    private readonly IBusService busService;
    private readonly IIdentityProvider identityProvider;
    private readonly string publishMarketDataTopic;
    private readonly string addStockRequestTopic;
    private readonly string refreshStockRequestTopic;

    public MarketController(IStockService stockService, IOptionService optionService, IBusService busService,
        IIdentityProvider identityProvider, IOptions<NatsSettings> options)
    {
        this.stockService = stockService;
        this.optionService = optionService;
        this.busService = busService;
        this.identityProvider = identityProvider;
        this.publishMarketDataTopic = options.Value.PublishMarketDataTopic;
        this.addStockRequestTopic = options.Value.AddStockRequestTopic;
        this.refreshStockRequestTopic = options.Value.RefreshStockRequestTopic;
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
    /// Adds stock to the system
    /// </summary>
    [HttpPost("Stocks/{ticker}"), Authorize("publishing")]
    public Task AddStockAsync(string ticker)
    {
        return this.busService.PublishAsync(this.addStockRequestTopic, ticker);
    }

    /// <summary>
    /// Refresh stock data
    /// </summary>
    [HttpPut("Stocks/{ticker}"), Authorize("publishing")]
    public Task RefreshStockAsync(string ticker)
    {
        return this.busService.PublishAsync(this.refreshStockRequestTopic, ticker);
    }

    /// <summary>
    /// Publish market data
    /// </summary>
    [HttpPost("Publish"), Authorize("publishing")]
    public Task PublishAsync()
    {
        return this.busService.PublishAsync(this.publishMarketDataTopic);
    }
}