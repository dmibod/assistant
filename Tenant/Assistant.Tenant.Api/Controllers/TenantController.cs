namespace Assistant.Tenant.Api.Controllers;

using Assistant.Tenant.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TenantController : ControllerBase
{
    private readonly IPositionService positionService;

    public TenantController(IPositionService positionService)
    {
        this.positionService = positionService;
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