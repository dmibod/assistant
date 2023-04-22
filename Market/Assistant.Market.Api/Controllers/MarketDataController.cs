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

    [HttpPost("Add")]
    public Task<Stock> CreateAsync(string ticker)
    {
        return this.stockService.GetOrCreateAsync(ticker);
    }

    [HttpPost("Publish")]
    public Task PublishAsync()
    {
        return Task.CompletedTask;
    }
}