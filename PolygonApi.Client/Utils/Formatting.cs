namespace Polygon.Client.Utils;

public static class Formatting
{
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
}