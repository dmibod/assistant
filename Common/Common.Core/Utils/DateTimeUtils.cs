namespace Common.Core.Utils;

public static class DateTimeUtils
{
    public static DateTime TodayUtc()
    {
        return Today(DateTimeKind.Utc);
    }

    public static DateTime Today()
    {
        return Today(DateTimeKind.Local);
    }
    
    private static DateTime Today(DateTimeKind kind)
    {
        var now = kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
        return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, kind);
    }
}