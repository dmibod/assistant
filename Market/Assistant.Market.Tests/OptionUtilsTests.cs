namespace Assistant.Market.Tests;

using Common.Core.Utils;

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
    
    [DataTestMethod]
    [DynamicData(nameof(ParseExpirationData))]
    public void ParseExpiration_ReturnsExpectedResult(string expiration, DateTime expectedValue)
    {
        // Arrange & Act
        var actualValue = OptionUtils.ParseExpiration(expiration);
        
        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    public static IEnumerable<object[]> ParseExpirationData
    {
        get
        {
            return new[]
            {
                new object[] { "19700101", new DateTime(1970, 1, 1, 0, 0, 0) },
                new object[] { "20230404", new DateTime(2023, 4, 4, 0, 0, 0) }
            };
        }
    }
}