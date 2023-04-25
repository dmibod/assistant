namespace Assistant.Tenant.Api.Controllers;

using System.Security.Claims;
using Assistant.Tenant.Core.Services;
using Common.Core.Security;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TenantController : ControllerBase
{
    private readonly IPositionService positionService;
    private readonly ITenantService tenantService;
    private readonly IIdentityProvider identityProvider;

    public TenantController(IPositionService positionService, ITenantService tenantService, IIdentityProvider identityProvider)
    {
        this.positionService = positionService;
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

        var claim = identity.Claims.Single(c => c.Type == "exp").Value;
        var value = long.Parse(claim);
        var exp = DateTime.UnixEpoch.AddSeconds(value);

        var result = new
        {
            User = tenant.Name,
            Expiration = exp.ToLongDateString()
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
}