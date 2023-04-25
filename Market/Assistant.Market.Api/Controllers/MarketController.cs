namespace Assistant.Market.Api.Controllers;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Configuration;
using Common.Core.Services;
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
    private readonly string publishMarketDataTopic;
    private readonly string addStockRequestTopic;

    public MarketController(IStockService stockService, IOptionService optionService, IBusService busService, IOptions<NatsSettings> options)
    {
        this.stockService = stockService;
        this.optionService = optionService;
        this.busService = busService;
        this.publishMarketDataTopic = options.Value.PublishMarketDataTopic;
        this.addStockRequestTopic = options.Value.AddStockRequestTopic;
    }

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

    [HttpGet("OptionChain")]
    public async Task<ActionResult> GetOptionChainAsync(string ticker)
    {
        var chain = await this.optionService.FindAsync(ticker);

        return this.Ok(chain);
    }

    [HttpPost("AddStock"), Authorize("publishing")]
    public Task<Stock> AddStockAsync(string ticker)
    {
        return this.stockService.GetOrCreateAsync(ticker);
    }

    [HttpPost("Queue/AddStock"), Authorize("publishing")]
    public Task QueueAddStockAsync(string ticker)
    {
        return this.busService.PublishAsync(this.addStockRequestTopic, ticker);
    }

    [HttpPost("Queue/Publish"), Authorize("publishing")]
    public Task QueuePublishAsync()
    {
        return this.busService.PublishAsync(this.publishMarketDataTopic);
    }
}