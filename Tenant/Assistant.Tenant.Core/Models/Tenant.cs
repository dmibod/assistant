namespace Assistant.Tenant.Core.Models;

public class Tenant
{
    public string Name { get; set; }

    public string DefaultFilter { get; set; }
    
    public string SellPutsBoardId { get; set; }
    
    public string SellCallsFilter { get; set; }
    
    public string SellCallsBoardId { get; set; }

    public string PositionsBoardId { get; set; }
    
    public string OpenInterestFilter { get; set; }
    
    public string OpenInterestBoardId { get; set; }

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
    
    public string CardId { get; set; }
}

public enum AssetType
{
    Stock,
    Option
}