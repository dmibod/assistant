namespace Helper.Core.Utils;

public static class CalculationUtils
{
    public static decimal Roi(decimal investment, decimal profit)
    {
        return profit / investment;
    }
    
    public static decimal AnnualRoi(decimal investment, decimal profit, TimeSpan investmentPeriod)
    {
        var days = investmentPeriod.Days == 0 ? 1 : investmentPeriod.Days;
        
        var dailyRoi = Roi(investment, profit) / days;
        
        return dailyRoi * 365;
    }

    public static decimal AnnualRoiPercent(decimal investment, decimal profit, TimeSpan investmentPeriod, int? roundDigits = null)
    {
        return Percent(AnnualRoi(investment, profit, investmentPeriod), roundDigits);
    }

    public static decimal Percent(decimal ratio, int? roundDigits = null)
    {
        var percent = ratio * 100;
        
        return roundDigits.HasValue ? Math.Round(percent, roundDigits.Value) : percent;
    }

    public static decimal PercentOf(decimal percent, decimal amount)
    {
        return amount * percent / 100.0m;
    }
}