namespace Common.Core.Tests.Utils;

using Common.Core.Utils;

[TestClass]
public class ProgressTrackerTests
{
    [TestMethod]
    public void Increase_TotalItemsIsZero_DoesNotThrowException()
    {
        // Arrange
        var tracker = new ProgressTracker(0, 1, _ => { });
        
        // Act
        try
        {
            tracker.Increase();
        }
        catch (Exception e)
        {
            // Assert
            Assert.Fail();
        }
    }
}