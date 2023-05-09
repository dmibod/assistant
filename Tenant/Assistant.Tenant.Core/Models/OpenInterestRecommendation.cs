namespace Assistant.Tenant.Core.Models;

public class OpenInterestRecommendation
{
    public string Ticker { get; set; }
    
    public decimal OpenInterest { get; set; }
    
    public decimal PrevOpenInterest { get; set; }
    
    public decimal OpenInterestChange { get; set; }
    
    public decimal OpenInterestChangePercent { get; set; }
    
    public decimal Bid { get; set; }
    
    public decimal Ask { get; set; }
    
    public decimal Last { get; set; }
    
    public decimal Vol { get; set; }
    
    public int DaysTillExpiration { get; set; }
}