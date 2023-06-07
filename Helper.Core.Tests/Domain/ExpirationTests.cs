namespace Helper.Core.Tests.Domain;

using Helper.Core.Domain;

[TestClass]
public class ExpirationTests
{
    [DataTestMethod]
    [DataRow("20230616", true)]
    [DataRow("20230630", false)]
    public void IsMonthly_ReturnsExpectedValue(string expiration, bool expected)
    {
        // Arrange
        var exp = Expiration.FromYYYYMMDD(expiration);

        // Act
        var actual = exp.IsMonthly;

        // Assert
        Assert.AreEqual(expected, actual);
    }
}