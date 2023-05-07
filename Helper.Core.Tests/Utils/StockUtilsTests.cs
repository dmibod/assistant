namespace Helper.Core.Tests.Utils;

using Helper.Core.Utils;

[TestClass]
public class StockUtilsTests
{
    [DataTestMethod]
    [DataRow("aAPL", false)]
    [DataRow(null, false)]
    [DataRow("", false)]
    [DataRow("  ", false)]
    [DataRow("AAP1", false)]
    [DataRow("_AAPL", false)]
    [DataRow("AAPL", true)]
    public void IsValid_ReturnsExpectedResults(string stockTicker, bool expectedResult)
    {
        // Arrange & Act
        var actualResult = StockUtils.IsValid(stockTicker);
        
        // Assert
        Assert.AreEqual(expectedResult, actualResult);
    }
}