namespace HistoricalDataApi.Client;

using System.Text.Json.Serialization;

public class OptionsResponse
{
    [JsonPropertyName("code")] 
    public string Ticker { get; set; }

    [JsonPropertyName("exchange")] 
    public string Exchange { get; set; }
    
    [JsonPropertyName("lastTradeDate")]
    public string LastTradeDate { get; set; }

    [JsonPropertyName("lastTradePrice")]
    public decimal LastTradePrice { get; set; }

    [JsonPropertyName("data")]
    public OptionExpirationData[] Expirations { get; set; }
}

public class OptionExpirationData
{
    [JsonPropertyName("expirationDate")]
    public string ExpirationDate { get; set; }

    [JsonPropertyName("impliedVolatility")]
    public decimal ImpliedVolatility { get; set; }

    [JsonPropertyName("putVolume")] 
    public decimal PutVolume { get; set; }

    [JsonPropertyName("callVolume")] 
    public decimal CallVolume { get; set; }

    [JsonPropertyName("putCallVolumeRatio")] 
    public decimal PutCallVolumeRatio { get; set; }

    [JsonPropertyName("putOpenInterest")] 
    public decimal PutOpenInterest { get; set; }

    [JsonPropertyName("callOpenInterest")] 
    public decimal CallOpenInterest { get; set; }

    [JsonPropertyName("putCallOpenInterestRatio")] 
    public decimal PutCallOpenInterestRatio { get; set; }

    [JsonPropertyName("optionsCount")] 
    public int OptionsCount { get; set; }

    [JsonPropertyName("options")] 
    public OptionContractsData Options { get; set; }
}

public class OptionContractsData
{
    [JsonPropertyName("CALL")] 
    public OptionContractData[] Calls { get; set; }

    [JsonPropertyName("PUT")] 
    public OptionContractData[] Puts { get; set; }
}

public class OptionContractData
{
    [JsonPropertyName("contractName")]
    public string ContractName { get; set; }

    [JsonPropertyName("contractSize")]
    public string ContractSize { get; set; }

    [JsonPropertyName("contractPeriod")] 
    public string ContractPeriod { get; set; }

    [JsonPropertyName("currency")] 
    public string Currency { get; set; }

    [JsonPropertyName("type")] 
    public string Type { get; set; }

    [JsonPropertyName("inTheMoney")] 
    public string InTheMoney { get; set; }

    [JsonPropertyName("lastTradeDateTime")] 
    public string LastTradeDateTime { get; set; }

    [JsonPropertyName("expirationDate")] 
    public string ExpirationDate { get; set; }

    [JsonPropertyName("strike")] 
    public decimal Strike { get; set; }

    [JsonPropertyName("lastPrice")] 
    public decimal LastPrice { get; set; }
    
    [JsonPropertyName("bid")] 
    public decimal? Bid { get; set; }
    
    [JsonPropertyName("ask")] 
    public decimal? Ask { get; set; }
    
    [JsonPropertyName("change")] 
    public decimal? Change { get; set; }
    
    [JsonPropertyName("changePercent")] 
    public decimal? ChangePercent { get; set; }
    
    [JsonPropertyName("volume")] 
    public decimal? Volume { get; set; }

    [JsonPropertyName("openInterest")] 
    public decimal? OpenInterest { get; set; }

    [JsonPropertyName("impliedVolatility")]
    public decimal ImpliedVolatility { get; set; }

    [JsonPropertyName("delta")] 
    public decimal Delta { get; set; }

    [JsonPropertyName("gamma")] 
    public decimal Gamma { get; set; }

    [JsonPropertyName("theta")] 
    public decimal Theta { get; set; }

    [JsonPropertyName("vega")] 
    public decimal Vega { get; set; }

    [JsonPropertyName("rho")] 
    public decimal Rho { get; set; }

    [JsonPropertyName("theoretical")] 
    public decimal Theoretical { get; set; }
    
    [JsonPropertyName("intrinsicValue")] 
    public decimal IntrinsicValue { get; set; }

    [JsonPropertyName("timeValue")] 
    public decimal TimeValue { get; set; }

    [JsonPropertyName("updatedAt")] 
    public string UpdatedAt { get; set; }

    [JsonPropertyName("daysBeforeExpiration")] 
    public int DaysTillExpiration { get; set; }
}