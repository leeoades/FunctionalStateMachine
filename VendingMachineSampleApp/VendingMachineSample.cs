using FunctionalStateMachine.CommandRunner;
using Microsoft.Extensions.DependencyInjection;
using VendingMachineSampleApp.Configuration;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp;

/// <summary>
/// Interactive vending machine sample demonstrating the functional state machine pattern.
///
/// Key characteristics of the functional approach:
/// - Data is NOT stored in the state machine
/// - Fire() takes current state, data, and trigger
/// - Fire() returns new state, new data, and commands
/// - The caller is responsible for maintaining state and data
/// - State machine is pure - no side effects during transitions
/// 
/// This demonstrates:
/// - Complete state machine with complex guards and state transitions
/// - Multiple command types with different handlers
/// - Dependency injection with automatic handler scanning
/// - Real-world domain modeling with state data mutation
/// </summary>
public class VendingMachineSample
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Clear();
        DisplayWelcome();

        // Initialize inventory
        var inventory = new Dictionary<string, VendingItem>
        {
            ["A1"] = new("A1", "Crisps", 1.50m, 5),
            ["A2"] = new("A2", "Snickers", 0.75m, 10),
            ["B1"] = new("B1", "Fanta", 2.00m, 3),
            ["B2"] = new("B2", "Water", 1.00m, 8),
            ["C1"] = new("C1", "Biscuits", 1.25m, 4)
        };

        // Set up dependency injection to register command handlers
        var services = new ServiceCollection();
        var exitSignal = new ExitSignal();
        RegisterCommandHandlers(services, inventory, exitSignal);
        var provider = services.BuildServiceProvider();

        // Get the dispatcher for VendingMachineCommand
        var dispatcher = provider.GetRequiredService<ICommandDispatcher<VendingMachineCommand>>();

        // Build the state machine (pure - no data yet)
        var machine = VendingMachineBuilder.BuildMachine();

        // Initialize machine data (data is EXTERNAL to the state machine)
        var machineData = VendingMachineData.Initialize(inventory);
        var currentState = VendingMachineState.Idle;

        // Create mutable wrapper to pass through async calls
        var session = new MachineSession { CurrentState = currentState, CurrentData = machineData };

        dispatcher.Run([new ShowInventoryCommand()]);

        // Start interactive session
        RunInteractiveSession(machine, dispatcher, session, exitSignal);
    }

    /// <summary>
    /// Registers all command handlers using dependency injection with automatic discovery.
    /// Uses AddCommandRunners to scan for and register all ICommandRunner implementations.
    /// </summary>
    private static void RegisterCommandHandlers(
        IServiceCollection services,
        Dictionary<string, VendingItem> inventory,
        ExitSignal exitSignal)
    {
        // Register inventory as singleton for handlers that need it
        services.AddSingleton(inventory);
        services.AddSingleton(exitSignal);

        services.AddCommandRunners<VendingMachineCommand>();
    }

    /// <summary>
    /// Displays welcome screen with ASCII art.
    /// </summary>
    private static void DisplayWelcome()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         Welcome to the Vending Machine Simulator!          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.ResetColor();
    }

    /// <summary>
    /// Displays the current inventory with item codes and prices.
    /// </summary>
    /// <summary>
    /// Runs the interactive vending machine session.
    /// Data and state are maintained by the caller through the MachineSession wrapper and passed to Fire().
    /// This demonstrates the functional state machine pattern.
    /// </summary>
    private static void RunInteractiveSession(
        FunctionalStateMachine.Core.StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand> machine,
        ICommandDispatcher<VendingMachineCommand> dispatcher,
        MachineSession session,
        ExitSignal exitSignal)
    {
        while (!exitSignal.ShouldExit)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter command: ");
            Console.ResetColor();

            var input = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";
            var trigger = ParseTrigger(input, session.CurrentData.Inventory);
            try
            {
                (session.CurrentState, session.CurrentData, var commands) =
                    machine.Fire(trigger, session.CurrentState, session.CurrentData);
                dispatcher.Run(commands);
            }
            catch (InvalidOperationException)
            {
                dispatcher.Run([
                    new DisplayMessageCommand("That action is not valid right now.")
                ]);
            }

            if (exitSignal.ShouldExit)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("👋 Thank you for using the vending machine!");
                Console.ResetColor();
                break;
            }
        }
    }

    private static VendingMachineTrigger ParseTrigger(string input, Dictionary<string, VendingItem> inventory)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new InvalidInputTrigger(input);
        }

        if (input == "EXIT")
        {
            return new ExitTrigger();
        }

        if (input == "HELP")
        {
            return new ShowInventoryTrigger();
        }

        if (input == "CANCEL")
        {
            return new CancelTrigger();
        }

        if (decimal.TryParse(input, out var amount) && amount > 0)
        {
            return new InsertMoneyTrigger(amount);
        }

        if (input.Length == 2 && inventory.ContainsKey(input))
        {
            return new SelectItemTrigger(input);
        }

        return new InvalidInputTrigger(input);
    }
}

public class MachineSession
{
    public VendingMachineState CurrentState { get; set; }
    public required VendingMachineData CurrentData { get; set; }
}
