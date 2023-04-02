namespace PolygonApi.Client.Tests;

using PolygonApi.Client.Utils;

[TestClass]
public class FormattingTests
{
    [TestMethod]
    public void ToExpiration_ReturnsExpectedResult()
    {
        // Arrange
        var dateTime = new DateTime(1970, 01, 31);
        
        // Act
        var expiration = Formatting.ToExpiration(dateTime);
        
        // Assert
        Assert.AreEqual("700131", expiration);
    }

    [TestMethod]
    public void ToPriceBarDateTime_ReturnsExpectedResult()
    {
        // Arrange
        var dateTime = new DateTime(2022, 01, 31, 18, 0, 11);
        
        // Act
        var actual = Formatting.ToPriceBarDateTime(dateTime);
        
        // Assert
        Assert.AreEqual("20220131 18:00:11", actual);
    }

    [TestMethod]
    public void FromNanosecondsTimestamp_ReturnsExpectedResult()
    {
        // Arrange
        var timestamp = 1605195918507251700;
        
        // Act
        var dateTime = Formatting.FromNanosecondsTimestamp(timestamp);
        
        // Assert
        var actual = Formatting.ToPriceBarDateTime(dateTime);
        
        Assert.AreEqual("20201112 15:45:18", actual);
    }
}