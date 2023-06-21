namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;
using PolygonApi.Client;
using PolygonApi.Client.Utils;

public class MarketDataService : IMarketDataService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<MarketDataService> logger;

    public MarketDataService(IHttpClientFactory httpClientFactory, ILogger<MarketDataService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    private ApiClient ApiClient => new(this.httpClientFactory.CreateClient("PolygonApiClient"));

    public async Task<AssetPrice?> GetStockPriceAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.GetStockPriceAsync), ticker);

        var response = await this.ApiClient.PrevCloseAsync(new PrevCloseRequest
        {
            Ticker = ticker
        });

        return response.AsAssetPrice();
    }

    public async Task<OptionChain?> GetOptionChainAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.GetOptionChainAsync), ticker);

        var resultChain = new OptionChain
        {
            Ticker = ticker,
            Expirations = new Dictionary<string, OptionExpiration>()
        };

        var optionChain = await this.GetOptionChainDataAsync(ticker);

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
                    resultContracts.Call =
                        ToContract(
                            OptionUtils.OptionTicker(ticker, expiration, Formatting.FormatStrike(strikePrice), true),
                            call);
                }

                var put = expirationOptions.FirstOrDefault(item =>
                    item.Details.StrikePrice == strikePrice && item.Details.ContractType == "put");
                if (put != null)
                {
                    resultContracts.Put =
                        ToContract(
                            OptionUtils.OptionTicker(ticker, expiration, Formatting.FormatStrike(strikePrice), false),
                            put);
                }
            }
        }

        return resultChain;
    }

    private async Task<Dictionary<string, List<OptionChainItemResponse>>> GetOptionChainDataAsync(string ticker)
    {
        var results = new List<OptionChainResponse>();

        try
        {
            var stream = this.ApiClient.OptionChainStreamAsync(new OptionChainRequest { Ticker = ticker });

            await foreach (var response in stream)
            {
                if (response != null)
                {
                    results.Add(response);
                }
            }

            return results.SelectMany(item => item.Results)
                .Where(item => item.Day != null && item.Day.Close > decimal.Zero && item.Details != null && !string.IsNullOrEmpty(item.Details.ExpirationDate))
                .GroupBy(item => item.Details.ExpirationDate)
                .ToDictionary(item => item.Key.Replace("-", string.Empty), item => item.ToList());
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);

            throw;
        }
    }

    private static OptionContract ToContract(string ticker, OptionChainItemResponse item)
    {
        return new OptionContract
        {
            Ticker = ticker,
            Bid = item.Day.Low,
            Ask = item.Day.High,
            Last = item.Day.Close,
            Vol = item.Day.Volume,
            OI = item.OpenInterest,
            TimeStamp = Formatting.FromNanosecondsTimestamp(item.Day.LastUpdated),
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
            TimeStamp = Formatting.FromMillisecondsTimestamp(priceItem.Timestamp)
        };
    }
}