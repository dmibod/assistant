namespace Assistant.Tenant.Core.Models;

public class Tenant
{
    public string Name { get; set; }
    
    public WatchListItem[] WatchList { get; set; }

    public Position[] Positions { get; set; }
}

public class WatchListItem
{
    public string Ticker { get; set; }

    public decimal BuyPrice { get; set; }

    public decimal SellPrice { get; set; }
}

public class Position
{
    public string Account { get; set; }
    
    public string Ticker { get; set; }

    public AssetType Type { get; set; }
    
    public int Quantity { get; set; }

    public string Tag { get; set; }

    public decimal AverageCost { get; set; }
}

public enum AssetType
{
    Stock,
    Option
}