namespace Helper.Core.Tests.Domain;

using Helper.Core.Domain;

[TestClass]
public class StrikeOptionIdTests
{
    [TestMethod]
    public void CallFromCurrentMonth_WhenCalled_ThenCurrentMonthIsUsed()
    {
        // Arrange
        var currentMonth = DateTime.UtcNow.Month;
        const string ticker = "AAPL";
        const decimal strike = 100.0m;
        
        // Act
        var id = StockOptionId.CallFromCurrentMonth(ticker, strike);
        var expirationMonth = (int)id.Expiration.Month;

        // Assert
        Assert.AreEqual(currentMonth, expirationMonth);
    }
    
    [TestMethod]
    public void CallFromCurrentYear_WhenCalled_ThenCurrentYearIsUsed()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        const Months month = Months.April;
        const string ticker = "AAPL";
        const decimal strike = 100.0m;
        
        // Act
        var id = StockOptionId.CallFromCurrentYear(ticker, strike, 1, month);
        var expirationYear = id.Expiration.Year;

        // Assert
        Assert.AreEqual(currentYear, expirationYear);
    }
}