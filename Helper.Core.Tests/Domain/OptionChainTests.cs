namespace Helper.Core.Tests.Domain;

using Helper.Core.Domain;
using Helper.Core.Specification;
using Helper.Core.Utils;

[TestClass]
public class OptionChainTests
{
    [TestMethod]
    public void FindOptions_When_Then()
    {
        // Arrange
        const string ticker = "AAPL";
        const decimal price = 110.0m;
        var stock = Stock.From(ticker);
        stock.Price = MarketPrice.From(price);

        var chain = OptionChain.For(stock, Expiration.FromNow(365 / 5), new[] { 90.0m, 95.0m, 100.0m, 105.0m, 110.0m, 115.0m });

        // Act
        var maxAnnualRoi = chain
            .FindOptions(new ExpressionSpecification<StockOption>(opt => SellForAPercentOfStrike(opt).BreakEvenStockPrice <= 90.0m))
            .Max(opt => SellForAPercentOfStrike(opt).AnnualRoi);

        // Assert
        Assert.AreEqual(5.0m, CalculationUtils.Percent(maxAnnualRoi, 0));
    }
    
    private static SellOperation SellForAPercentOfStrike(StockOption opt)
    {
        var sellPrice = CalculationUtils.PercentOf(1, opt.Id.Strike);
        return opt.Sell(sellPrice);
    }
}