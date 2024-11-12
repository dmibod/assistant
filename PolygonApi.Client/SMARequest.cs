namespace PolygonApi.Client;

public class SMARequest
{
    public string Ticker { get; set; }

    public DateTime Timestamp { get; set; }
    
    public int Size { get; set; }
    
    public int Limit { get; set; }
}