﻿namespace Assistant.Market.Core.Models;

public class Stock
{
    public string Ticker { get; set; }
    
    public decimal? Bid { get; set; }
    
    public decimal? Ask { get; set; }
    
    public decimal? Last { get; set; }

    public string[] Strikes { get; set; }
    
    public string[] Expirations { get; set; }
    
    public DateTime? LastRefresh { get; set; }
}