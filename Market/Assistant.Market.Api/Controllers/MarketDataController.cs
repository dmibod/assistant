namespace Assistant.Market.Api.Controllers;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class MarketDataController : ControllerBase
{
    private readonly IStockService stockService;
    private readonly IOptionService optionService;

    public MarketDataController(IStockService stockService, IOptionService optionService)
    {
        this.stockService = stockService;
        this.optionService = optionService;
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

    [HttpPost("AddStock")]
    public Task<Stock> AddStockAsync(string ticker)
    {
        return this.stockService.GetOrCreateAsync(ticker);
    }

    [HttpPost("Publish")]
    public Task PublishAsync()
    {
        return Task.CompletedTask;
    }
}