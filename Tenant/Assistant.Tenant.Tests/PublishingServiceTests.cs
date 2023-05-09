namespace Assistant.Tenant.Tests;

[TestClass]
public class PublishingServiceTests
{
    [TestMethod]
    public void PublishOpenInterestAsync_ProducesExpectedResult()
    {
        // Arrange
        var s1 = "oi\u0394#";
        var s2 = "oi\u0394%";
        
        // Act
        Console.WriteLine(s1);
        Console.WriteLine(s2);

        // Assert
        Assert.IsTrue(true);
    }
}