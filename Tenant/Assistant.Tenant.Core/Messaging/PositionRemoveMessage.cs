namespace Assistant.Tenant.Core.Messaging;

using Common.Core.Messaging;
using Common.Core.Messaging.Models;

public class PositionRemoveMessage : TenantMessage
{
    public string Account { get; set; }
    
    public string Ticker { get; set; }
}