using StockPurchaserSampleApp.Domain;

namespace StockPurchaserSampleApp.Runtime;

public sealed class InMemoryStockStore
{
    private readonly Dictionary<string, (StockPurchaserState State, StockPurchaserData Data)> _storage = new(StringComparer.OrdinalIgnoreCase);

    public (StockPurchaserState State, StockPurchaserData Data) Load(string symbol)
    {
        if (_storage.TryGetValue(symbol, out var stored))
        {
            return stored;
        }

        var initial = (StockPurchaserState.Idle, StockPurchaserData.Create(symbol));
        _storage[symbol] = initial;
        return initial;
    }

    public void Save(string symbol, StockPurchaserState state, StockPurchaserData data)
    {
        _storage[symbol] = (state, data);
    }
}
