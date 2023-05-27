namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class OptionService : IOptionService
{
    private readonly IOptionRepository repository;
    private readonly IOptionChangeRepository changeRepository;
    private readonly ILogger<OptionService> logger;

    public OptionService(IOptionRepository repository, IOptionChangeRepository changeRepository,
        ILogger<OptionService> logger)
    {
        this.repository = repository;
        this.changeRepository = changeRepository;
        this.logger = logger;
    }

    public async Task<OptionChain> FindAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindAsync), ticker);

        ticker = StockUtils.Format(ticker);

        var options = await this.repository.FindByTickerAsync(ticker);

        return options.AsChain(ticker);
    }

    public async Task<OptionExpiration?> FindExpirationAsync(string ticker, string expiration)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindExpirationAsync), $"{ticker}-{expiration}");

        ticker = StockUtils.Format(ticker);
        
        var option = await this.repository.FindExpirationAsync(ticker, expiration);

        if (option == null)
        {
            return null;
        }
        
        var array = new[] { option };
        
        return array.AsEnumerable().AsExpiration(expiration);
    }

    public async Task<OptionChain> FindChangeAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindChangeAsync), ticker);

        ticker = StockUtils.Format(ticker);

        var options = await this.changeRepository.FindByTickerAsync(ticker);

        return options.AsChain(ticker);
    }

    public Task<IEnumerable<string>> FindExpirationsAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindExpirationsAsync), ticker);

        return this.repository.FindExpirationsAsync(StockUtils.Format(ticker));
    }

    public async Task UpdateAsync(OptionChain options)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateAsync), $"{options.Ticker}-{options.Expirations.Count}");

        foreach (var expiration in options.Expirations.Keys)
        {
            var option = options.Expirations[expiration].AsOption(options.Ticker);

            option.LastRefresh = DateTime.UtcNow;

            if (await this.repository.ExistsAsync(options.Ticker, expiration))
            {
                var values = await this.repository.FindExpirationAsync(options.Ticker, expiration);

                await this.repository.UpdateAsync(option);

                var change = new Option
                {
                    Ticker = option.Ticker,
                    Expiration = option.Expiration,
                    LastRefresh = DateTime.UtcNow,
                    Contracts = Difference(values.Contracts, option.Contracts)
                        .Where(item => item.OI != decimal.Zero)
                        .OrderByDescending(item => Math.Abs(item.OI))
                        .ToArray()
                };

                if (change.Contracts.Length > 0)
                {
                    await this.changeRepository.CreateOrUpdateAsync(change);
                }
            }
            else
            {
                await this.repository.CreateAsync(option);
            }
        }
    }

    private static IEnumerable<OptionContract?> Difference(OptionContract[] prev, OptionContract[] next)
    {
        var oldContracts = prev.ToDictionary(contract => contract.Ticker);

        foreach (var newContract in next)
        {
            var ticker = newContract.Ticker;

            if (!oldContracts.ContainsKey(ticker))
            {
                continue;
            }
            
            var difference = Difference(oldContracts[ticker], newContract);

            if (difference != null)
            {
                yield return difference;
            }
        }
    }

    private static OptionContract? Difference(OptionContract prev, OptionContract next)
    {
        if (prev.TimeStamp == next.TimeStamp)
        {
            return null;
        }

        var oiDiff = next.OI - prev.OI;
        
        return new OptionContract
        {
            Ticker = next.Ticker,
            Ask = next.Ask,
            Bid = next.Bid,
            Last = next.Last,
            // we store OI delta % here
            Vol = prev.OI == decimal.Zero ? decimal.MaxValue : CalculationUtils.Percent(oiDiff / prev.OI, 2),
            OI = oiDiff,
            TimeStamp = next.TimeStamp
        };
    }

    public async Task RemoveAsync(IDictionary<string, ISet<string>> expirations)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), expirations.Count);

        try
        {
            await this.repository.RemoveAsync(expirations);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }

        try
        {
            await this.changeRepository.RemoveAsync(expirations);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }
    }

    public async Task RemoveAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), ticker);

        ticker = StockUtils.Format(ticker);

        try
        {
            await this.repository.RemoveAsync(ticker);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }

        try
        {
            await this.changeRepository.RemoveAsync(ticker);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }
    }

    public Task<int> FindChangesCountAsync(string ticker, Func<DateTime> todayFn)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindChangesCountAsync), ticker);

        return this.changeRepository.FindChangesCountAsync(StockUtils.Format(ticker), todayFn);
    }

    public Task<decimal> FindOpenInterestChangeMinAsync(string ticker, Func<DateTime> todayFn)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOpenInterestChangeMinAsync), ticker);

        return this.changeRepository.FindOpenInterestMinAsync(StockUtils.Format(ticker), todayFn);
    }

    public Task<decimal> FindOpenInterestChangeMaxAsync(string ticker, Func<DateTime> todayFn)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOpenInterestChangeMaxAsync), ticker);

        return this.changeRepository.FindOpenInterestMaxAsync(StockUtils.Format(ticker), todayFn);
    }
    
    public Task<decimal> FindOpenInterestChangePercentMinAsync(string ticker, Func<DateTime> todayFn)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOpenInterestChangePercentMinAsync), ticker);

        return this.changeRepository.FindOpenInterestPercentMinAsync(StockUtils.Format(ticker), todayFn);
    }

    public Task<decimal> FindOpenInterestChangePercentMaxAsync(string ticker, Func<DateTime> todayFn)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOpenInterestChangePercentMaxAsync), ticker);

        return this.changeRepository.FindOpenInterestPercentMaxAsync(StockUtils.Format(ticker), todayFn);
    }

    public Task<IEnumerable<OptionChange>> FindTopsAsync(string ticker, int count, Func<DateTime> todayFn)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindTopsAsync), $"{ticker}-{count}");

        return this.changeRepository.FindTopsAsync(StockUtils.Format(ticker), count, todayFn);
    }
}