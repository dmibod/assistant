namespace Assistant.Market.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class MarketDataController : ControllerBase
{
    public MarketDataController()
    {
    }

    [HttpPost("Publish")]
    public async Task PublishAsync()
    {
    }
}