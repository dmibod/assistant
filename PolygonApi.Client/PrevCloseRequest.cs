namespace PolygonApi.Client;

public class PrevCloseRequest
{
    public string Ticker { get; set; }
    public string Expiration { get; set; }
    public string Side { get; set; }
    public decimal Strike { get; set; }
}