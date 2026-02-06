using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles ShowInventoryCommand by rendering the current inventory and menu options.
/// </summary>
public class ShowInventoryHandler(Dictionary<string, VendingItem> inventory) : ICommandRunner<ShowInventoryCommand>
{
    public void Run(ShowInventoryCommand command)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("📦 Available Items:");
        Console.WriteLine("─────────────────────────────────────");
        foreach (var (code, item) in inventory.OrderBy(x => x.Key))
        {
            var stock = item.Quantity > 0 ? $"{item.Quantity} in stock" : "OUT OF STOCK";
            Console.WriteLine($"  {code}  {item.Name,-15} £{item.Price,-5:F2}  ({stock})");
        }
        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine("  HELP - Show this menu");
        Console.WriteLine("  EXIT - Quit the program");
        Console.ResetColor();
    }
}
