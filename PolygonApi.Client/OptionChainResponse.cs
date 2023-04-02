namespace PolygonApi.Client;

using System.Text.Json.Serialization;

public class OptionChainResponse
{
    [JsonPropertyName("request_id")] 
    public string RequestId { get; set; }

    [JsonPropertyName("status")] 
    public string Status { get; set; }
    
    [JsonPropertyName("results")]
    public OptionChainItemResponse[] Results { get; set; }

    [JsonPropertyName("next_url")]
    public string? NextUrl { get; set; }
}

public class OptionChainItemResponse
{
  [JsonPropertyName("break_even_price")]
  public decimal BreakEvenPrice { get; set; }

  [JsonPropertyName("implied_volatility")]
  public decimal ImpliedVolatility { get; set; }

  [JsonPropertyName("open_interest")]
  public decimal OpenInterest { get; set; }

  [JsonPropertyName("day")]
  public OptionChainDayResponse Day { get; set; }

  [JsonPropertyName("details")]
  public OptionChainDetailsResponse Details { get; set; }

  [JsonPropertyName("greeks")]
  public OptionChainGreeksResponse Greeks { get; set; }

  [JsonPropertyName("last_quote")]
  public OptionChainLastQuoteResponse LastQuote { get; set; }

  [JsonPropertyName("last_trade")]
  public OptionChainLastTradeResponse LastTrade { get; set; }

  [JsonPropertyName("underlying_asset")]
  public OptionChainUnderlyingAssetResponse UnderlyingAsset { get; set; }
}

public class OptionChainDayResponse
{
  [JsonPropertyName("change")]
  public decimal Change { get; set; }
  
  [JsonPropertyName("change_percent")]
  public decimal ChangePercent { get; set; }
  
  [JsonPropertyName("close")]
  public decimal Close { get; set; }
  
  [JsonPropertyName("high")]
  public decimal High { get; set; }
  
  [JsonPropertyName("last_updated")]
  public long LastUpdated { get; set; }
  
  [JsonPropertyName("low")]
  public decimal Low { get; set; }
  
  [JsonPropertyName("open")]
  public decimal Open { get; set; }
  
  [JsonPropertyName("previous_close")]
  public decimal PreviousClose { get; set; }
  
  [JsonPropertyName("volume")]
  public decimal Volume { get; set; }
  
  [JsonPropertyName("vwap")]
  public decimal Vwap { get; set; }
}

public class OptionChainDetailsResponse
{
  [JsonPropertyName("contract_type")]
  public string ContractType { get; set; }
  
  [JsonPropertyName("exercise_style")]
  public string ExerciseStyle { get; set; }
  
  [JsonPropertyName("expiration_date")]
  public string ExpirationDate { get; set; }
  
  [JsonPropertyName("shares_per_contract")]
  public decimal SharesPerContract { get; set; }
  
  [JsonPropertyName("strike_price")]
  public decimal StrikePrice { get; set; }
  
  [JsonPropertyName("ticker")]
  public string Ticker { get; set; }
}

public class OptionChainGreeksResponse
{
  [JsonPropertyName("delta")]
  public decimal Delta { get; set; }
  
  [JsonPropertyName("gamma")]
  public decimal Gamma { get; set; }
  
  [JsonPropertyName("theta")]
  public decimal Theta { get; set; }
  
  [JsonPropertyName("vega")]
  public decimal Vega { get; set; }
}

public class OptionChainLastQuoteResponse
{
  [JsonPropertyName("ask")]
  public decimal Ask { get; set; }
  
  [JsonPropertyName("ask_size")]
  public decimal AskSize { get; set; }
  
  [JsonPropertyName("bid")]
  public decimal Bid { get; set; }
  
  [JsonPropertyName("bid_size")]
  public decimal BidSize { get; set; }
  
  [JsonPropertyName("last_updated")]
  public long LastUpdated { get; set; }
  
  [JsonPropertyName("midpoint")]
  public decimal Midpoint { get; set; }
}

public class OptionChainLastTradeResponse
{
  [JsonPropertyName("conditions")]
  public int[] Conditions { get; set; }
  
  [JsonPropertyName("exchange")]
  public decimal Exchange { get; set; }
  
  [JsonPropertyName("price")]
  public decimal Price { get; set; }
  
  [JsonPropertyName("sip_timestamp")]
  public long SipTimestamp { get; set; }
  
  [JsonPropertyName("size")]
  public decimal Size { get; set; }
  
  [JsonPropertyName("timeframe")]
  public string Timeframe { get; set; }
}

public class OptionChainUnderlyingAssetResponse
{
  [JsonPropertyName("change_to_break_even")]
  public decimal ChangeToBreakEven { get; set; }
  
  [JsonPropertyName("last_updated")]
  public long LastUpdated { get; set; }
  
  [JsonPropertyName("price")]
  public decimal Price { get; set; }

  [JsonPropertyName("ticker")]
  public string Ticker { get; set; }
  
  [JsonPropertyName("timeframe")]
  public string Timeframe { get; set; }
}
