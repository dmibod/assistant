namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
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

        ticker = ticker.ToUpper();

        var options = await this.repository.FindByTickerAsync(ticker);

        return options.AsChain(ticker);
    }

    public async Task<OptionChain> FindChangeAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindChangeAsync), ticker);

        ticker = ticker.ToUpper();

        var options = await this.changeRepository.FindByTickerAsync(ticker);

        return options.AsChain(ticker);
    }

    public Task<IEnumerable<string>> FindExpirationsAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindExpirationsAsync), ticker);

        return this.repository.FindExpirationsAsync(ticker);
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
                    Contracts = Difference(values.Contracts, option.Contracts).Where(item => item.OI != decimal.Zero).ToArray()
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

        return new OptionContract
        {
            Ticker = next.Ticker,
            Ask = next.Ask - prev.Ask,
            Bid = next.Bid - prev.Bid,
            Last = next.Last - prev.Last,
            Vol = next.Vol - prev.Vol,
            OI = next.OI - prev.OI,
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
}