using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles DisplayMessageCommand by outputting messages to the console.
/// Simulates updating a vending machine display screen.
/// </summary>
public class DisplayMessageHandler : ICommandRunner<DisplayMessageCommand>
{
    public void Run(DisplayMessageCommand command)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"🖥️  {command.Message}");
        Console.ResetColor();
    }
}
