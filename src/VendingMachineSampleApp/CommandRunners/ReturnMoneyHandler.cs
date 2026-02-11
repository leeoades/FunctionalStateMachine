using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles ReturnMoneyCommand by simulating the coin return slot dispensing money.
/// In a real system, this would trigger the coin mechanism to dispense physical coins or bills.
/// </summary>
public class ReturnMoneyHandler : ICommandRunner<ReturnMoneyCommand>
{
    public void Run(ReturnMoneyCommand command)
    {
        if (command.Amount <= 0)
            return;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"💰 Returning change: £{command.Amount:F2}");
        Console.WriteLine($"🪙 *coins dispensed*");
        
        // Simulate coin return mechanism
        Thread.Sleep(300);
        
        Console.ResetColor();
    }
}
