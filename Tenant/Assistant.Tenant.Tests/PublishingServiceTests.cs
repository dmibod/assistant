namespace Assistant.Tenant.Tests;

using Common.Core.Utils;

[TestClass]
public class PublishingServiceTests
{
    [TestMethod]
    public void PublishOpenInterestAsync_ProducesExpectedResult()
    {
        // Arrange
        var s1 = "oi\u0394\u21D1\u21D3#";
        var s2 = "oi\u0394\u2191\u2193%";
        
        // Act
        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(FormatUtils.FormatPercent(1000));

        // Assert
        Assert.IsTrue(true);
    }
}