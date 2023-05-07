namespace Helper.Core.Utils;

using System.Text.RegularExpressions;

public static partial class StockUtils
{
    public static string Format(string rawStockTicker)
    {
        var stockTicker = rawStockTicker.ToUpper();

        return IsValid(stockTicker) ? stockTicker : throw new FormatException($"Invalid stock ticker {rawStockTicker}");
    }

    public static bool IsValid(string stockTicker)
    {
        return !string.IsNullOrWhiteSpace(stockTicker) && StockTickerRegex().IsMatch(stockTicker);
    }

    [GeneratedRegex("^[A-Z]+$")]
    private static partial Regex StockTickerRegex();
}