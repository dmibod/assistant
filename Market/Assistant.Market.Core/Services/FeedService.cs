﻿namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Microsoft.Extensions.Logging;

public class FeedService : IFeedService
{
    private readonly IMarketDataService marketDataService;
    private readonly IStockService stockService;
    private readonly IOptionService optionService;
    private readonly ILogger<FeedService> logger;

    public FeedService(IMarketDataService marketDataService, IStockService stockService, IOptionService optionService, ILogger<FeedService> logger)
    {
        this.marketDataService = marketDataService;
        this.stockService = stockService;
        this.optionService = optionService;
        this.logger = logger;
    }

    public async Task FeedAsync(TimeSpan lag)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FeedAsync), lag.ToString());

        var stock = await this.stockService.FindOldestAsync(lag);
        if (stock != null)
        {
            await this.UpdateStockAsync(stock);
        }
    }

    private async Task UpdateStockAsync(Stock stock)
    {
        var stockPrice = await this.marketDataService.GetStockPriceAsync(stock.Ticker);

        if (stockPrice != null)
        {
            stock.Ask = stockPrice.Ask;
            stock.Bid = stockPrice.Bid;
            stock.Last = stockPrice.Last;
            stock.LastRefresh = DateTime.UtcNow;
            
            await this.stockService.UpdateAsync(stock);
        }

        var optionChain = await this.marketDataService.GetOptionChainAsync(stock.Ticker);

        if (optionChain != null)
        {
            await this.optionService.UpdateAsync(optionChain);
        }
    }
}