﻿namespace Assistant.Tenant.Core.Models;

public class Tenant
{
    public string Name { get; set; }
    
    public IEnumerable<WatchItem> WatchList { get; set; }

    public IEnumerable<Position> Positions { get; set; }
}

public class WatchItem
{
    public string Ticker { get; set; }

    public decimal BuyPrice { get; set; }

    public decimal SellPrice { get; set; }
}

public class Position
{
    public string Account { get; set; }
    
    public string Asset { get; set; }

    public AssetType Type { get; set; }
    
    public int Quantity { get; set; }

    public decimal AverageCost { get; set; }
}

public enum AssetType
{
    Stock,
    Option
}