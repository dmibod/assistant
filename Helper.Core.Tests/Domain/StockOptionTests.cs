namespace Helper.Core.Tests.Domain;

using Helper.Core.Domain;

[TestClass]
public class StockOptionTests
{
    [TestMethod]
    public void InTheMoney_WhenStockPriceAboveStrike_ThenCallOptionInTheMoney()
    {
        // Arrange
        const string ticker = "AAPL";
        const decimal price = 110.0m;
        
        var stock = Stock.From(ticker);
        stock.Price = MarketPrice.From(price);
        
        const decimal strike = 100.0m;
        var option = StockOption.Call(stock, strike, Expiration.Now);

        // Act & Assert
        Assert.IsTrue(option.InTheMoney);
    }
    
    [TestMethod]
    public void InTheMoney_WhenStockPriceBelowStrike_ThenPutOptionInTheMoney()
    {
        // Arrange
        const string ticker = "AAPL";
        const decimal price = 110.0m;
        
        var stock = Stock.From(ticker);
        stock.Price = MarketPrice.From(price);
        
        const decimal strike = 120.0m;
        var option = StockOption.Put(stock, strike, Expiration.Now);

        // Act & Assert
        Assert.IsTrue(option.InTheMoney);
    }
}