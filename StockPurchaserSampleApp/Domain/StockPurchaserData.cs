namespace StockPurchaserSampleApp.Domain;

public sealed record StockPurchaserData(
    string Symbol,
    decimal? TargetBuyPrice,
    decimal LastPrice,
    bool Purchased)
{
    public static StockPurchaserData Create(string symbol) =>
        new(symbol, null, 0m, false);
}
