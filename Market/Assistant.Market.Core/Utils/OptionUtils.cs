namespace Assistant.Market.Core.Utils;

using System.Text.RegularExpressions;

public static class OptionUtils
{
    const string Pattern = @"(.+)(\d{8})([C|P])(\d{8})";

    public static string OptionTicker(string stockTicker, string expiration, string strike, bool isCall)
    {
        var side = isCall ? "C" : "P";
        
        return $"{stockTicker}{expiration}{side}{strike}";
    }

    public static decimal GetStrike(string optionTicker)
    {
        var match = Regex.Match(optionTicker, Pattern);
        if (!match.Success)
        {
            throw new Exception($"Can't get strike price from option ticker '{optionTicker}'");
        }
        
        var strike = match.Groups[4].Value;
        return new decimal(int.Parse(strike) / 1000.0);
    }
    
    public static string GetSide(string optionTicker)
    {
        var match = Regex.Match(optionTicker, Pattern);
        if (!match.Success)
        {
            throw new Exception($"Can't get contract side from option ticker '{optionTicker}'");
        }
        
        return match.Groups[3].Value;
    }
}