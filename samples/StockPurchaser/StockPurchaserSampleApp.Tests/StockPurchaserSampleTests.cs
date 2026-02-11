using StockPurchaserSampleApp.Configuration;
using StockPurchaserSampleApp.Domain;
using StockPurchaserSampleApp.Runtime;

namespace StockPurchaserSampleApp.Tests;

public class StockPurchaserSampleTests
{
    [Fact]
    public void PriceTick_BelowTarget_RemainsTracking()
    {
        var machine = StockPurchaserMachine.Build();
        var store = new InMemoryStockStore();
        var symbol = "ACME";
        var (state, data) = store.Load(symbol);

        var (state1, data1, _) = machine.Fire(new SetTargetPriceTrigger(100m), state, data);
        store.Save(symbol, state1, data1);

        var (state2, data2, _) = machine.Fire(new PriceTickTrigger(105m), state1, data1);

        Assert.Equal(StockPurchaserState.Tracking, state2);
        Assert.Equal(105m, data2.LastPrice);
        Assert.False(data2.Purchased);
    }

    [Fact]
    public void PriceTick_AtOrBelowTarget_Purchases()
    {
        var machine = StockPurchaserMachine.Build();
        var symbol = "BETA";
        var data = StockPurchaserData.Create(symbol);

        var (state1, data1, _) = machine.Fire(new SetTargetPriceTrigger(100m), StockPurchaserState.Idle, data);
        var (state2, data2, _) = machine.Fire(new PriceTickTrigger(95m), state1, data1);

        Assert.Equal(StockPurchaserState.Purchased, state2);
        Assert.True(data2.Purchased);
    }

    [Fact]
    public void PriceTick_AfterPurchase_DoesNotThrow()
    {
        var machine = StockPurchaserMachine.Build();
        var data = StockPurchaserData.Create("COHO");

        var (state1, data1, _) = machine.Fire(new SetTargetPriceTrigger(100m), StockPurchaserState.Idle, data);
        var (state2, data2, _) = machine.Fire(new PriceTickTrigger(90m), state1, data1);
        var (state3, data3, _) = machine.Fire(new PriceTickTrigger(95m), state2, data2);

        Assert.Equal(StockPurchaserState.Purchased, state3);
        Assert.True(data3.Purchased);
    }
}
