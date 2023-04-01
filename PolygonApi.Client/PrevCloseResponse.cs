namespace Polygon.Client;

using System.Text.Json.Serialization;

public class PrevCloseResponse
{
    [JsonPropertyName("adjusted")]
    public bool Adjusted { get; set; }

    [JsonPropertyName("queryCount")]
    public int QueryCount { get; set; }

    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    [JsonPropertyName("resultsCount")]
    public int ResultsCount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; }

    [JsonPropertyName("results")]
    public PrevCloseItemResponse[] Results { get; set; }
}

public class PrevCloseItemResponse
{
    [JsonPropertyName("T")]
    public string AssetId { get; set; }

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