namespace Assistant.Market.Api.Controllers;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class MarketDataController : ControllerBase
{
    private readonly IStockService stockService;

    public MarketDataController(IStockService stockService)
    {
        this.stockService = stockService;
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