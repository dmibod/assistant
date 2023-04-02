namespace PolygonApi.Client.Utils;

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

    public static string ToExpiration(DateTime dateTime)
    {
        return dateTime.ToString("yyMMdd");
    }
    
    public static string ToPriceBarDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyyMMdd HH:mm:ss");
    }

    public static DateTime GetNextWeekday(DayOfWeek day)
    {
        var today = DateTime.Today;
        var daysToAdd = ((int) day - (int) today.DayOfWeek + 7) % 7;
        
        return today.AddDays(daysToAdd);
    }

    public static DateTime FromNanosecondsTimestamp(long nanosecondsTimestamp)
    {
        var ticks = nanosecondsTimestamp / 100;
        
        return DateTime.UnixEpoch.AddTicks(ticks);
    }
}