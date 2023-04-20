namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Microsoft.Extensions.Logging;
using PolygonApi.Client;
using PolygonApi.Client.Utils;

public class FeedService : IFeedService
{
    private readonly ApiClient apiClient;
    private readonly IStockService stockService;
    private readonly ILogger<FeedService> logger;

    public FeedService(ApiClient apiClient, IStockService stockService, ILogger<FeedService> logger)
    {
        this.apiClient = apiClient;
        this.stockService = stockService;
        this.logger = logger;
    }

    public async Task FeedAsync(TimeSpan lag)
    {
        this.logger.LogInformation("{Method} with lag {Argument}", nameof(this.FeedAsync), lag.ToString());

        var stock = await this.stockService.FindOutdatedWithLagAsync(lag);

        if (stock != null)
        {
            await this.UpdateStockAsync(stock);
        }
    }

    private async Task UpdateStockAsync(Stock stock)
    {
        var response = await this.apiClient.PrevCloseAsync(new PrevCloseRequest
        {
            Ticker = stock.Ticker
        });

        this.logger.LogInformation("{Result}", response.Status);

        var optionChain = this.apiClient
            .OptionChainStream(new OptionChainRequest { Ticker = stock.Ticker })
            .SelectMany(item => item.Results)
            .Where(item => item.Day.Close > decimal.Zero)
            .GroupBy(item => item.Details.ExpirationDate)
            .ToDictionary(item => item.Key.Replace("-", string.Empty), item => item.ToList());

        foreach (var expiration in /*stock.Expirations*/optionChain.Keys)
        {
            if (!optionChain.ContainsKey(expiration))
            {
                continue;
            }

            var expirationOptions = optionChain[expiration];

            foreach (var strikePrice in /*stock.Strikes*/this.GetStrikes(expirationOptions.Select(i => i.Details.StrikePrice)))
            {
                var call = expirationOptions.FirstOrDefault(item =>
                    item.Details.StrikePrice == strikePrice && item.Details.ContractType == "call");
                if (call != null)
                {
                    var price = ToPriceHistory(call);
                }

                var put = expirationOptions.FirstOrDefault(item =>
                    item.Details.StrikePrice == strikePrice && item.Details.ContractType == "put");
                if (put != null)
                {
                    var price = ToPriceHistory(put);
                }
            }
        }
    }

    private Stock ToPriceHistory(OptionChainItemResponse item)
    {
        var timeStamp = Formatting.FromNanosecondsTimestamp(item.Day.LastUpdated);
        var time = Formatting.ToPriceBarDateTime(timeStamp);

        this.logger.LogInformation("{Asset}", item.Details.Ticker);

        return new Stock
        {
            Ticker = $"{item.Details.ContractType.Substring(0, 1).ToUpper()}|{item.Details.StrikePrice}",
            Bid = item.Day.Low,
            Ask = item.Day.High,
            Last = item.Day.Close
        };
    }

    private IEnumerable<decimal> GetStrikes(IEnumerable<decimal> values)
    {
        var hs = new HashSet<decimal>();
        return values.Where(value => hs.Add(value));
    }
}