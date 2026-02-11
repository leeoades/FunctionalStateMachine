using StockPurchaserSampleApp.Domain;

namespace StockPurchaserSampleApp.Runtime;

public sealed class CommandExecutor
{
    public void Execute(IEnumerable<StockPurchaserCommand> commands)
    {
        foreach (var command in commands)
        {
            switch (command)
            {
                case LogCommand log:
                    if (log.Color.HasValue)
                        Console.ForegroundColor = log.Color.Value;
                    Console.WriteLine(log.Message);
                    Console.ResetColor();
                    break;
                case ExecutePurchaseCommand purchase:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[BUY] {purchase.Symbol} at {purchase.Price:C}");
                    Console.ResetColor();
                    break;
            }
        }
    }
}
