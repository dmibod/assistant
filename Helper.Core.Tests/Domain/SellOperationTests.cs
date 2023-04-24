namespace Helper.Core.Tests.Domain;

using Helper.Core.Domain;
using Helper.Core.Utils;

[TestClass]
public class SellOperationTests
{
    [TestMethod]
    public void Roi_WhenCalculated_ThenReturnsExpectedValue()
    {
        // Arrange
        const string ticker = "AAPL";
        const decimal stockPrice = 100.0m;
        var stock = Stock.From(ticker);
        stock.Price = MarketPrice.From(stockPrice);
        
        const decimal strike = 100.0m;
        var option = StockOption.Put(stock, strike, Expiration.FromNow(365 / 5));
        const decimal optionPrice = 1.0m;
        option.Price = MarketPrice.From(optionPrice);

        var operation = new SellOperation(option, optionPrice);

        // Act & Assert
        Assert.AreEqual(5.0m, CalculationUtils.Percent(operation.AnnualRoi, 0));
    }
}