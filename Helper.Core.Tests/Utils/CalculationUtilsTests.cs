namespace Helper.Core.Tests.Utils;

using Helper.Core.Utils;

[TestClass]
public class CalculationUtilsTests
{
    public static IEnumerable<object[]> AnnualRoiTestData
    {
        get
        {
            return new[]
            { 
                new object[] { 100.0m, 5.0m, TimeSpan.FromDays(365), 5.0m },
                new object[] { 100.0m, 5.0m, TimeSpan.FromDays(365 / 5), 25.0m },
                new object[] { 100.0m, 10.0m, TimeSpan.FromDays(365 * 2), 5.0m }
            };
        }
    }
    
    [TestMethod]
    [DynamicData(nameof(AnnualRoiTestData))]
    public void AnnualRoi_WhenCalculated_ThenReturnsExpectedValue(decimal investment, decimal profit, TimeSpan investmentPeriod, decimal expectedRoi)
    {
        // Arrange & Act
        var actualRoi = CalculationUtils.AnnualRoiPercent(investment, profit, investmentPeriod, 0);
        
        // Assert
        Assert.AreEqual(expectedRoi, actualRoi);
    }

    [TestMethod]
    public void Test()
    {
        var strike = 15;
        var investment = strike * 100;
        var profit = 231 - 1.05m;
        var enterDate = new DateTime(2023, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var exitDate = new DateTime(2023, 6, 16, 0, 0, 0, DateTimeKind.Utc);
        var investmentPeriod = exitDate - enterDate;
        
        var roi = CalculationUtils.AnnualRoiPercent(investment, profit, investmentPeriod, 2);
        
        Console.WriteLine("ROI: " + roi);
    }
}