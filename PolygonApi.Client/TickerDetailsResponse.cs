namespace PolygonApi.Client;

using System.Text.Json.Serialization;

public class TickerDetailsResponse
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("results")]
    public TickerDetailsItemResponse Results { get; set; }
}

public class TickerDetailsItemResponse
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("address")]
    public AddressResponse Address { get; set; }

    [JsonPropertyName("branding")]
    public BrandingResponse Branding { get; set; }

    [JsonPropertyName("cik")]
    public string Cik { get; set; }

    [JsonPropertyName("composite_figi")]
    public string CompositeFigi { get; set; }

    [JsonPropertyName("currency_name")]
    public string CurrencyName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("homepage_url")]
    public string HomepageUrl { get; set; }

    [JsonPropertyName("list_date")]
    public string ListDate { get; set; }

    [JsonPropertyName("locale")]
    public string Locale { get; set; }

    [JsonPropertyName("market")]
    public string Market { get; set; }

    [JsonPropertyName("market_cap")]
    public double MarketCap { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; }

    [JsonPropertyName("primary_exchange")]
    public string PrimaryExchange { get; set; }

    [JsonPropertyName("round_lot")]
    public long RoundLot { get; set; }

    [JsonPropertyName("share_class_figi")]
    public string ShareClassFigi { get; set; }

    [JsonPropertyName("share_class_shares_outstanding")]
    public long ShareClassSharesOutstanding { get; set; }

    [JsonPropertyName("sic_code")]
    public string SicCode { get; set; }

    [JsonPropertyName("sic_description")]
    public string SicDescription { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; }

    [JsonPropertyName("ticker_root")]
    public string TickerRoot { get; set; }

    [JsonPropertyName("total_employees")]
    public long TotalEmployees { get; set; }

    [JsonPropertyName("type")]
    public string TickerType { get; set; }

    [JsonPropertyName("weighted_shares_outstanding")]
    public long WeightedSharesOutstanding { get; set; }
}

public class AddressResponse {
    [JsonPropertyName("address1")]
    public string Address1 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }
}

public class BrandingResponse {
    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }

    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; }
}
