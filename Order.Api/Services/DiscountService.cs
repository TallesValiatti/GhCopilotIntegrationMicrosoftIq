namespace Order.Api.Services;

public static class DiscountService
{
    public static decimal GetDiscountRate(decimal grossTotal) =>
        grossTotal switch
        {
            < 1000m => 0m,
            < 2000m => 0.10m,
            < 3000m => 0.15m,
            _ => 0.20m
        };
}
