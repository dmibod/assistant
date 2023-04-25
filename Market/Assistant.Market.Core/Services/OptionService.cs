namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Microsoft.Extensions.Logging;

public class OptionService : IOptionService
{
    private readonly IOptionRepository repository;
    private readonly ILogger<OptionService> logger;

    public OptionService(IOptionRepository repository, ILogger<OptionService> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<OptionChain> FindAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindAsync), ticker);

        ticker = ticker.ToUpper();
        
        var options = await this.repository.FindByTickerAsync(ticker);

        return options.AsChain(ticker);
    }

    public async Task UpdateAsync(OptionChain options)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateAsync), options.Ticker);

        foreach (var expiration in options.Expirations.Keys)
        {
            var optionExpiration = options.Expirations[expiration];
            var option = optionExpiration.AsOption(options.Ticker);
            
            if (await this.repository.ExistsAsync(options.Ticker, expiration))
            {
                await this.repository.UpdateAsync(option);
            }
            else
            {
                await this.repository.CreateAsync(option);
            }
        }
    }

    public Task<IEnumerable<string>> FindExpirationsAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindExpirationsAsync), ticker);

        return this.repository.FindExpirationsAsync(ticker);
    }

    public Task RemoveAsync(IDictionary<string, ISet<string>> expirations)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), expirations.Count);

        return this.repository.RemoveAsync(expirations);
    }
}