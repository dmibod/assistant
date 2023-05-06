namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Models;
using Common.Core.Messaging;

public class PositionCreateMessage : Position, ITenantAware
{
    public string Tenant { get; set; }
    
    public Position AsPosition()
    {
        return new Position
        {
            Account = this.Account,
            Quantity = this.Quantity,
            Ticker = this.Ticker,
            Tag = this.Tag,
            Type = this.Type,
            AverageCost = this.AverageCost
        };
    }
}