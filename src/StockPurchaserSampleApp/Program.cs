using StockPurchaserSampleApp.Configuration;
using StockPurchaserSampleApp.Domain;
using StockPurchaserSampleApp.Runtime;

namespace StockPurchaserSampleApp;

public static class Program
{
    public static void Main()
    {
        var machine = StockPurchaserMachine.Build();
        var store = new InMemoryStockStore(); // Simulated Actor persistence
        var executor = new CommandExecutor(); // Handroll your own command runner if you like

        var symbols = new[] { "ACME", "BETA", "COHO" };
        var random = new Random();

        // Setup some target prices for the stock. If it dips below that price, auto purchase some.
        Console.WriteLine("Starting stock purchaser sample. Press Ctrl+C to exit.");
        foreach (var symbol in symbols)
        {
            var target = random.Next(90, 111);
            Dispatch(symbol, new SetTargetPriceTrigger(target), machine, store, executor);
        }

        // Little game loop that simulates some price fluctuations
        var ticks = 30;
        while (ticks-- > 0)
        {
            foreach (var symbol in symbols)
            {
                if (random.NextDouble() > 0.1) continue; // Change prices randomly
                
                var price = random.Next(80, 121);
                Dispatch(symbol, new PriceTickTrigger(price), machine, store, executor);
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        Console.WriteLine("Sample run complete.");
    }

    private static void Dispatch(
        string symbol,
        StockPurchaserTrigger trigger,
        FunctionalStateMachine.Core.StateMachine<StockPurchaserState, StockPurchaserTrigger, StockPurchaserData, StockPurchaserCommand> machine,
        InMemoryStockStore store,
        CommandExecutor executor)
    {
        var (state, data) = store.Load(symbol);
        var (newState, newData, commands) = machine.Fire(trigger, state, data);
        store.Save(symbol, newState, newData);
        executor.Execute(commands);
    }
}