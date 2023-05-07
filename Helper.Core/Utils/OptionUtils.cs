namespace Helper.Core.Utils;

using System.Text.RegularExpressions;

public static class OptionUtils
{
    private const string OptionTickerPattern = @"^([A-Z]+)(\d{8})([C|P])(\d{8})$";
    private const string ExpirationPattern = @"^(\d{4})(\d{2})(\d{2})$";

    public static string Format(string rawOptionTicker)
    {
        var optionTicker = rawOptionTicker.ToUpper();

        return IsValid(optionTicker) ? optionTicker : throw new FormatException($"Invalid option ticker {rawOptionTicker}");
    }

    public static bool IsValid(string optionTicker)
    {
        return !string.IsNullOrWhiteSpace(optionTicker) && Regex.IsMatch(optionTicker, OptionTickerPattern);
    }

    public static string OptionTicker(string stockTicker, string expiration, string strike, bool isCall)
    {
        var side = isCall ? "C" : "P";
        
        return $"{stockTicker}{expiration}{side}{strike}";
    }

    public static string OptionTicker(string stockTicker, string expiration, decimal strike, bool isCall)
    {
        return OptionTicker(stockTicker, expiration, FormatStrike(strike), isCall);
    }

    public static string GetStock(string optionTicker)
    {
        var match = Regex.Match(optionTicker, OptionTickerPattern);
        if (!match.Success)
        {
            throw new Exception($"Can't get stock from option ticker '{optionTicker}'");
        }
        
        return match.Groups[1].Value;
    }

    public static decimal GetStrike(string optionTicker)
    {
        var match = Regex.Match(optionTicker, OptionTickerPattern);
        if (!match.Success)
        {
            throw new Exception($"Can't get strike price from option ticker '{optionTicker}'");
        }
        
        var strike = match.Groups[4].Value;
        return new decimal(int.Parse(strike) / 1000.0);
    }
    
    public static string GetSide(string optionTicker)
    {
        var match = Regex.Match(optionTicker, OptionTickerPattern);
        if (!match.Success)
        {
            throw new Exception($"Can't get contract side from option ticker '{optionTicker}'");
        }
        
        return match.Groups[3].Value;
    }

    public static string GetExpiration(string optionTicker)
    {
        return AsYYYYMMDD(ParseExpiration(optionTicker));
    }

    public static string FormatStrike(decimal strike)
    {
        var value = $"{Math.Round(strike * 1000, 0)}";
        var leadingZeroes = 8 - value.Length;

        if (leadingZeroes < 0)
        {
            leadingZeroes = 0;
        }

        return new string('0', leadingZeroes) + value;
    }

    public static DateTime ParseExpiration(string expiration)
    {
        var match = Regex.Match(expiration, ExpirationPattern);
        if (!match.Success)
        {
            throw new Exception($"Can't parse date from expiration '{expiration}'");
        }
        
        var year = int.Parse(match.Groups[1].Value);
        var month = int.Parse(match.Groups[2].Value);
        var day = int.Parse(match.Groups[3].Value);
        
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }
    
    public static string AsYYYYMMDD(DateTime expiration)
    {
        return expiration.ToString("yyyyMMdd");
    }
}