namespace Assistant.Tenant.Api.Controllers;

using System.Security.Claims;
using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Services;
using Common.Core.Security;
using Common.Core.Utils;
using Common.Infrastructure.Security;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TenantController : ControllerBase
{
    private readonly IPositionService positionService;
    private readonly IPublishingService publishingService;
    private readonly ITenantService tenantService;
    private readonly IIdentityProvider identityProvider;

    public TenantController(IPositionService positionService, IPublishingService publishingService, ITenantService tenantService, IIdentityProvider identityProvider)
    {
        this.positionService = positionService;
        this.publishingService = publishingService;
        this.tenantService = tenantService;
        this.identityProvider = identityProvider;
    }

    [HttpPost("Token")]
    [EnableCors("CorsPolicy")]
    public async Task<ActionResult> Token()
    {
        var identity = this.identityProvider.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return this.BadRequest();
        }

        var tenant = await this.tenantService.GetOrCreateAsync();

        var result = new
        {
            User = tenant.Name,
            Expiration = identity.GetExpiration().ToLongDateString()
        };

        return this.Ok(result);
    }

    [HttpGet("Positions")]
    public async Task<ActionResult> GetPositionsAsync()
    {
        var positions = await this.positionService.FindAllAsync();

        var result = new
        {
            Count = positions.Count(),
            Items = positions.OrderBy(position => position.Account).ThenBy(position => position.Asset).ToArray()
        };

        return this.Ok(result);
    }

    [HttpPost("Positions/{account}/{ticker}")]
    public Task<Position> AddStockPositionAsync(string account, string ticker, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Asset = ticker.ToUpper(),
            Type = AssetType.Stock,
            AverageCost = averageCost,
            Quantity = size
        };

        return this.positionService.CreateAsync(position);
    }

    [HttpPost("Positions/{account}/{ticker}/Call/{yyyymmmdd}")]
    public Task<Position> AddCallOptionPositionAsync(string account, string ticker, string yyyymmmdd, decimal strike, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Asset = OptionUtils.OptionTicker(ticker.ToUpper(), yyyymmmdd, strike, true),
            Type = AssetType.Option,
            AverageCost = averageCost,
            Quantity = size,
            Tag = tag
        };

        return this.positionService.CreateAsync(position);
    }

    [HttpPost("Positions/{account}/{ticker}/Put/{yyyymmmdd}")]
    public Task<Position> AddPutOptionPositionAsync(string account, string ticker, string yyyymmmdd, decimal strike, decimal averageCost, int size, string tag = "")
    {
        var position = new Position
        {
            Account = account.ToUpper(),
            Asset = OptionUtils.OptionTicker(ticker.ToUpper(), yyyymmmdd, strike, false),
            Type = AssetType.Option,
            AverageCost = averageCost,
            Quantity = size,
            Tag = tag
        };

        return this.positionService.CreateAsync(position);
    }

    [HttpPut("Positions/{account}/{ticker}/{tag}")]
    public Task TagPositionAsync(string account, string ticker, string tag)
    {
        return this.positionService.UpdateTagAsync(account.ToUpper(), ticker.ToUpper(), tag);
    }

    [HttpDelete("Positions/{account}")]
    public Task RemovePositionAsync(string account, string ticker)
    {
        return this.positionService.RemoveAsync(account.ToUpper(), ticker.ToUpper());
    }

    [HttpPost("Positions/Publish")]
    public Task PublishPositions()
    {
        return this.publishingService.PublishPositionsAsync();
    }
}