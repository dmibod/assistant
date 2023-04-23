namespace Helper.Core.Domain;

using Helper.Core.Utils;

public class SellOperation
{
    private readonly decimal optionPrice;

    public SellOperation(StockOption option, decimal optionPrice)
    {
        this.Option = option;
        this.optionPrice = optionPrice;
    }

    public StockOption Option { get; }
    
    /* it is a price of a stock after option expiration and assignment
       for PUT: BreakEven = Strike - Premium, if you sell assigned stock at this price, you have no loss 
       for CALL: BreakEven = Strike + Premium, if you buy stock on market at this price, you have no loss
       for covered Call, Strike + Premium should be >= Buy Price (cost of your shares)
    */
    public decimal BreakEvenStockPrice => this.Option.Id.OptionType == OptionType.Put ? this.Option.Id.Strike - this.optionPrice : this.Option.Id.Strike + this.optionPrice;

    public decimal ContractPrice => this.optionPrice * this.Option.Stock.GetOptionContractSize();

    public decimal LinearDailyDecay => this.ContractPrice / this.Option.DaysTillExpiration;

    public decimal Roi => CalculationUtils.Roi(this.Option.Collateral, this.ContractPrice);

    public decimal AnnualRoi => CalculationUtils.AnnualRoi(this.Option.Collateral, this.ContractPrice, TimeSpan.FromDays(this.Option.DaysTillExpiration));
}
