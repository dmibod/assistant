using System.Text.Json.Serialization;

namespace PolygonApi.Client;

public class SMAResponse
{
    [JsonPropertyName("request_id")] 
    public string RequestId { get; set; }

    [JsonPropertyName("status")] 
    public string Status { get; set; }
    
    [JsonPropertyName("results")]
    public SMAItemResponse Results { get; set; }

    [JsonPropertyName("next_url")]
    public string? NextUrl { get; set; }
}

public class SMAItemResponse
{
    [JsonPropertyName("underlying")]
    public SMAItemUnderlying Underlying { get; set; }
    
    [JsonPropertyName("values")]
    public SMAItemValue[] Values { get; set; }
}

public class SMAItemUnderlying
{
    [JsonPropertyName("aggregates")]
    public SMAItemAggregate[] Aggregates { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class SMAItemAggregate
{
    [JsonPropertyName("o")]
    public decimal Open { get; set; }

    [JsonPropertyName("h")]
    public decimal High { get; set; }

    [JsonPropertyName("l")]
    public decimal Low { get; set; }

    [JsonPropertyName("c")]
    public decimal Close { get; set; }

    [JsonPropertyName("n")]
    public int TxNo { get; set; }

    [JsonPropertyName("t")]
    public long Timestamp { get; set; }

    [JsonPropertyName("v")]
    public decimal Volume { get; set; }

    [JsonPropertyName("vw")]
    public decimal VWap { get; set; }
}

public class SMAItemValue
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
  
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
    
}
