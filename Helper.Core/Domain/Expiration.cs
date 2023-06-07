namespace Helper.Core.Domain;

public readonly struct Expiration
{
    private Expiration(byte day, Months month, ushort year)
    {
        this.Day = day;
        this.Month = month;
        this.Year = year;
    }
    
    public static Expiration FromYYYYMMDD(string expiration)
    {
        if (expiration == null || expiration.Length < 8) throw new ArgumentException(nameof(expiration));
        var year = ushort.Parse(expiration.Substring(0, 4));
        var month = int.Parse(expiration.Substring(4, 2));
        var day = byte.Parse(expiration.Substring(6, 2));
        return new Expiration(day, (Months)month, year);
    }

    public static Expiration From(byte day, Months month, ushort year)
    {
        return new Expiration(day, month, year);
    }

    public static Expiration From(DateTime date)
    {
        return new Expiration((byte)date.Day, (Months)date.Month, (ushort)date.Year);
    }

    public static Expiration FromNow(ushort days)
    {
        return From(DateTime.UtcNow + TimeSpan.FromDays(days));
    }

    public static Expiration FromCurrentYear(byte day, Months months)
    {
        return new Expiration(day, months, (ushort)DateTime.UtcNow.Year);
    }

    public static Expiration Now => FromCurrentYear((byte)DateTime.UtcNow.Day, (Months) DateTime.UtcNow.Month);

    public byte Day { get; }
    
    public Months Month { get; }

    public ushort Year { get; }

    public bool IsMonthly => this.AsDate().DayOfWeek == DayOfWeek.Friday && this.Day > 14 && this.Day < 22;
    
    public ushort DaysTillExpiration => (ushort)(this.AsDate() - DateTime.UtcNow.Date).Days;
    
    public DateTime AsDate()
    {
        return new DateTime(this.Year, (int)this.Month, this.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}