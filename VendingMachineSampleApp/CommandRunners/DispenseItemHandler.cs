using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles DispenseItemCommand by simulating the physical dispensing of an item.
/// In a real system, this would trigger motor control hardware to dispense the product.
/// </summary>
public class DispenseItemHandler : ICommandRunner<DispenseItemCommand>
{
    public void Run(DispenseItemCommand command)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🔊 *motor whirring sounds*");
        Console.WriteLine($"📦 Dispensing {command.ItemCode}...");
        Console.WriteLine($"✓ Item dispensed successfully");
        Console.ResetColor();
    }
}
