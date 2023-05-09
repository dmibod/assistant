namespace Common.Core.Utils;

public static class FormatUtils
{
    public static string FormatPrice(decimal? price, int digits = 2)
    {
        return $"${Math.Round(price ?? decimal.Zero, digits)}";
    }

    public static string FormatPercent(decimal? percent, int digits = 2)
    {
        return $"{Math.Round(percent ?? decimal.Zero, digits)}%";
    }

    public static string FormatSize(decimal size)
    {
        return size < 0 ? $"short({Math.Abs(size)})" : $"long({size})";
    }

    public static string FormatExpiration(DateTime expiraion)
    {
        return $"{expiraion.Year}/{expiraion.Month}/{expiraion.Day}";
    }

    public static string FormatAccount(string value)
    {
        return value.Substring(0, 2) + new string(Enumerable.Repeat('*', value.Length - 2).ToArray());
    }
}