namespace Common.Core.Utils;

public static class FormatUtils
{
    private const int PercentInfinityThreshold = 10000;
    
    public static string FormatNumber(decimal? number, int digits = 0)
    {
        return $"{Math.Round(number ?? decimal.Zero, digits)}";
    }

    public static string FormatAbsNumber(decimal? number, int digits = 0)
    {
        return FormatNumber(Math.Abs(number ?? decimal.Zero), digits);
    }

    public static string FormatPrice(decimal? price, int digits = 2)
    {
        return $"${Math.Round(price ?? decimal.Zero, digits)}";
    }

    public static string FormatPercent(decimal? percent, int digits = 2)
    {
        var value = Math.Round(percent ?? decimal.Zero, digits);
        return value >= PercentInfinityThreshold ? "\u221E%" : $"{value}%";
    }

    public static string FormatAbsPercent(decimal? percent, int digits = 2)
    {
        return FormatPercent(Math.Abs(percent ?? decimal.Zero), digits);
    }

    public static string FormatSize(decimal size)
    {
        return size < 0 ? $"short({Math.Abs(size)})" : $"long({size})";
    }

    public static string FormatExpiration(DateTime expiration, bool shortYear = false)
    {
        var year = shortYear ? expiration.Year % 100 : expiration.Year;
        return $"{year}/{expiration.Month:00}/{expiration.Day:00}";
    }

    public static string FormatAccount(string value)
    {
        return value.Substring(0, 2) + new string(Enumerable.Repeat('*', value.Length - 2).ToArray());
    }
}