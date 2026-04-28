namespace Order.Api.Services;

public class DiscountService
{
    public (decimal DiscountRate, decimal DiscountAmount, decimal NetTotal) Calculate(decimal grossTotal)
    {
        var discountRate = grossTotal switch
        {
            < 1000m => 0.00m,
            < 2000m => 0.10m,
            < 3000m => 0.15m,
            _ => 0.20m
        };

        var discountAmount = grossTotal * discountRate;
        var netTotal = grossTotal - discountAmount;

        return (discountRate, discountAmount, netTotal);
    }
}
