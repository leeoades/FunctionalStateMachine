using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles LogTransactionCommand by writing transaction records to console.
/// In a real system, this would write to a file, database, or logging service.
/// </summary>
public class LogTransactionHandler : ICommandRunner<LogTransactionCommand>
{
    public void Run(LogTransactionCommand command)
    {
        var status = command.Success ? "✓ SUCCESS" : "✗ FAILED";
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"💾 [LOG] Transaction: {command.ItemCode} | Amount: ${command.AmountPaid:F2} | {status}");
        Console.ResetColor();
    }
}
