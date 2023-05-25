namespace Common.Core.Tests.Utils;

using Common.Core.Utils;
using Helper.Core.Utils;

[TestClass]
public class FormatUtilsTests
{
    [DataTestMethod]
    [DataRow("20220311", false, "2022/03/11")]
    [DataRow("20220311", true, "22/03/11")]
    public void FormatExpiration_ReturnsExpectedResult(string expiration, bool shortFlag, string expected)
    {
        // Arrange
        var date = OptionUtils.ParseExpiration(expiration);
        
        // Act
        var actual = FormatUtils.FormatExpiration(date, shortFlag);
            
        // Assert
        Assert.AreEqual(expected, actual);
    }
}