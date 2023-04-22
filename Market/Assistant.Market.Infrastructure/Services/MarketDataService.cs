namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Assistant.Market.Core.Utils;
using Microsoft.Extensions.Logging;
using PolygonApi.Client;
using PolygonApi.Client.Utils;

public class MarketDataService : IMarketDataService
{
    private readonly ApiClient apiClient;
    private readonly ILogger<MarketDataService> logger;

    public MarketDataService(ApiClient apiClient, ILogger<MarketDataService> logger)
    {
        this.apiClient = apiClient;
        this.logger = logger;
    }

    public async Task<AssetPrice?> GetStockPriceAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.GetStockPriceAsync), ticker);
        
        var response = await this.apiClient.PrevCloseAsync(new PrevCloseRequest
        {
            Ticker = ticker
        });

        return response.AsAssetPrice();
    }

    public Task<OptionChain?> GetOptionChainAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.GetOptionChainAsync), ticker);

        var resultChain = new OptionChain
        {
            Ticker = ticker,
            Expirations = new Dictionary<string, OptionExpiration>()
        };

        var optionChain = this.apiClient
            .OptionChainStream(new OptionChainRequest { Ticker = ticker })
            .SelectMany(item => item.Results)
            .Where(item => item.Day.Close > decimal.Zero)
            .GroupBy(item => item.Details.ExpirationDate)
            .ToDictionary(item => item.Key.Replace("-", string.Empty), item => item.ToList());

        foreach (var expiration in optionChain.Keys)
        {
            var resultExpiration = new OptionExpiration
            {
                Expiration = expiration,
                Contracts = new Dictionary<decimal, OptionContracts>()
            };
            resultChain.Expirations.Add(expiration, resultExpiration);

            var expirationOptions = optionChain[expiration];

            foreach (var strikePrice in GetStrikes(expirationOptions.Select(i => i.Details.StrikePrice)))
            {
                var resultContracts = new OptionContracts
                {
                    Strike = strikePrice
                };
                resultExpiration.Contracts.Add(strikePrice, resultContracts);

                var call = expirationOptions.FirstOrDefault(item =>
                    item.Details.StrikePrice == strikePrice && item.Details.ContractType == "call");

                if (call != null)
                {
                    var price = ToPriceHistory(call);
                    resultContracts.Call = new OptionContract
                    {
                        Ticker = OptionUtils.OptionTicker(ticker, expiration, Formatting.FormatStrike(strikePrice), true),
                        Ask = price.Ask,
                        Bid = price.Bid,
                        Last = price.Last
                    };
                }

                var put = expirationOptions.FirstOrDefault(item =>
                    item.Details.StrikePrice == strikePrice && item.Details.ContractType == "put");
                if (put != null)
                {
                    var price = ToPriceHistory(put);
                    resultContracts.Put = new OptionContract
                    {
                        Ticker = OptionUtils.OptionTicker(ticker, expiration, Formatting.FormatStrike(strikePrice), false),
                        Ask = price.Ask,
                        Bid = price.Bid,
                        Last = price.Last
                    };
                }
            }
        }

        return Task.FromResult<OptionChain?>(resultChain);
    }

    private static Stock ToPriceHistory(OptionChainItemResponse item)
    {
        var timeStamp = Formatting.FromNanosecondsTimestamp(item.Day.LastUpdated);
        var time = Formatting.ToPriceBarDateTime(timeStamp);

        return new Stock
        {
            Ticker = $"{item.Details.ContractType.Substring(0, 1).ToUpper()}|{item.Details.StrikePrice}",
            Bid = item.Day.Low,
            Ask = item.Day.High,
            Last = item.Day.Close
        };
    }

    private static IEnumerable<decimal> GetStrikes(IEnumerable<decimal> values)
    {
        var hs = new HashSet<decimal>();
        return values.Where(value => hs.Add(value));
    }
}

public static class PrevCloseResponseExtensions
{
    public static AssetPrice? AsAssetPrice(this PrevCloseResponse? response)
    {
        if (response == null)
        {
            return null;
        }

        var priceItem = response.Results.MaxBy(item => item.Timestamp);

        if (priceItem == null)
        {
            return null;
        }

        return new AssetPrice
        {
            Ask = priceItem.High,
            Bid = priceItem.Low,
            Last = priceItem.Close,
            Ticker = response.Ticker,
            TimeStamp = Formatting.FromNanosecondsTimestamp(priceItem.Timestamp)
        };
    }
}