namespace Assistant.Market.Tests;

using Assistant.Market.Core.Utils;

[TestClass]
public class OptionUtilsTests
{
    [DataTestMethod]
    [DataRow("UBER20230515C00033000", 33.0)]
    public void GetStrike_ReturnsExpectedResult(string optionTicker, double expectedStrike)
    {
        // Arrange & Act
        var actualStrike = (double)OptionUtils.GetStrike(optionTicker);
        
        // Assert
        Assert.AreEqual(expectedStrike, actualStrike);
    }
}