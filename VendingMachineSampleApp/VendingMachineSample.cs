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
    public static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Clear();
        DisplayWelcome();

        // Initialize inventory
        var inventory = new Dictionary<string, VendingItem>
        {
            ["A1"] = new("A1", "Chips", 1.50m, 5),
            ["A2"] = new("A2", "Candy", 0.75m, 10),
            ["B1"] = new("B1", "Soda", 2.00m, 3),
            ["B2"] = new("B2", "Water", 1.00m, 8),
            ["C1"] = new("C1", "Cookies", 1.25m, 4)
        };

        // Set up dependency injection to register command handlers
        var services = new ServiceCollection();
        RegisterCommandHandlers(services, inventory);
        var provider = services.BuildServiceProvider();

        // Get the dispatcher for VendingMachineCommand
        var dispatcher = provider.GetRequiredService<ICommandDispatcher<VendingMachineCommand>>();

        // Build the state machine (pure - no data yet)
        var machine = VendingMachineBuilder.BuildMachine();

        // Initialize machine data (data is EXTERNAL to the state machine)
        var machineState = VendingMachineData.Initialize(inventory);
        var currentState = VendingMachineState.Idle;

        // Create mutable wrapper to pass through async calls
        var session = new MachineSession { CurrentState = currentState, CurrentData = machineState };

        // Start interactive session
        await RunInteractiveSession(machine, dispatcher, inventory, session);
    }

    /// <summary>
    /// Registers all command handlers using dependency injection with automatic discovery.
    /// Uses AddCommandRunners to scan for and register all ICommandRunner implementations.
    /// </summary>
    private static void RegisterCommandHandlers(
        IServiceCollection services,
        Dictionary<string, VendingItem> inventory)
    {
        // Register inventory as singleton for handlers that need it
        services.AddSingleton(inventory);

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
    private static void DisplayInventory(Dictionary<string, VendingItem> inventory)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("📦 Available Items:");
        Console.WriteLine("─────────────────────────────────────");
        foreach (var (code, item) in inventory.OrderBy(x => x.Key))
        {
            var stock = item.Quantity > 0 ? $"{item.Quantity} in stock" : "OUT OF STOCK";
            Console.WriteLine($"  {code}  {item.Name,-15} ${item.Price,-5:F2}  ({stock})");
        }
        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine("  HELP - Show this menu");
        Console.WriteLine("  EXIT - Quit the program");
        Console.ResetColor();
    }

    /// <summary>
    /// Runs the interactive vending machine session.
    /// Data and state are maintained by the caller through the MachineSession wrapper and passed to Fire().
    /// This demonstrates the functional state machine pattern.
    /// </summary>
    private static async Task RunInteractiveSession(
        FunctionalStateMachine.Core.StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand> machine,
        ICommandDispatcher<VendingMachineCommand> dispatcher,
        Dictionary<string, VendingItem> inventory,
        MachineSession session)
    {
        DisplayInventory(inventory);

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter command: ");
            Console.ResetColor();

            var input = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";

            if (input == "EXIT")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("👋 Thank you for using the vending machine!");
                Console.ResetColor();
                break;
            }

            if (input == "HELP")
            {
                DisplayInventory(inventory);
                continue;
            }

            // Parse item selection (e.g., "A1")
            if (input.Length == 2 && inventory.ContainsKey(input))
            {
                await ProcessItemSelection(input, machine, dispatcher, session);
                continue;
            }

            // Parse money input (e.g., "1.50" or "2")
            if (decimal.TryParse(input, out var amount) && amount > 0)
            {
                await ProcessMoneyInsertion(amount, machine, dispatcher, session);
                continue;
            }

            // Cancel current transaction
            if (input == "CANCEL")
            {
                var (newState, newData, commands) = machine.Fire(new CancelTrigger(), session.CurrentState, session.CurrentData);
                session.CurrentState = newState;
                session.CurrentData = newData;
                dispatcher.Run(commands);
                Console.WriteLine($"Current state: {newState}");
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Invalid command. Type HELP for options.");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Processes item selection and guides user through payment if needed.
    /// Demonstrates firing triggers and updating state/data from results.
    /// </summary>
    private static async Task ProcessItemSelection(
        string itemCode,
        FunctionalStateMachine.Core.StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand> machine,
        ICommandDispatcher<VendingMachineCommand> dispatcher,
        MachineSession session)
    {
        // Functional approach: Fire returns new state, new data, and commands
        var (newState, newData, commands) = machine.Fire(
            new SelectItemTrigger(itemCode),
            session.CurrentState,
            session.CurrentData);

        // Update session state
        session.CurrentState = newState;
        session.CurrentData = newData;

        // Dispatch the commands
        dispatcher.Run(commands);
        Console.WriteLine($"Current state: {newState}");

        // If item was selected successfully, guide through payment
        if (newState == VendingMachineState.ItemSelected && newData.SelectedItemPrice.HasValue)
        {
            Console.WriteLine($"💵 Item cost: ${newData.SelectedItemPrice:F2}");
            Console.Write("Enter amount to insert ($): ");

            if (decimal.TryParse(Console.ReadLine() ?? "", out var amount) && amount > 0)
            {
                await ProcessMoneyInsertion(amount, machine, dispatcher, session);
            }
        }
    }

    /// <summary>
    /// Processes money insertion and handles state transitions through payment validation and dispensing.
    /// Shows how multiple Fire() calls cascade through the state machine.
    /// </summary>
    private static async Task ProcessMoneyInsertion(
        decimal amount,
        FunctionalStateMachine.Core.StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand> machine,
        ICommandDispatcher<VendingMachineCommand> dispatcher,
        MachineSession session)
    {
        // Insert money
        var (state1, data1, commands1) = machine.Fire(
            new InsertMoneyTrigger(amount),
            session.CurrentState,
            session.CurrentData);

        session.CurrentState = state1;
        session.CurrentData = data1;
        dispatcher.Run(commands1);

        // If we reached dispensing, trigger completion sequence
        if (state1 == VendingMachineState.DispensingItem)
        {
            Console.WriteLine("💳 Processing payment...");
            await Task.Delay(500);

            var (state2, data2, commands2) = machine.Fire(
                new DispenseCompleteTrigger(),
                session.CurrentState,
                session.CurrentData);

            session.CurrentState = state2;
            session.CurrentData = data2;
            dispatcher.Run(commands2);

            if (state2 == VendingMachineState.ReturningChange)
            {
                await Task.Delay(500);
                var (state3, data3, commands3) = machine.Fire(
                    new DispenseCompleteTrigger(),
                    session.CurrentState,
                    session.CurrentData);
                session.CurrentState = state3;
                session.CurrentData = data3;
                dispatcher.Run(commands3);
                Console.WriteLine($"Current state: {state3}");
            }
            else
            {
                Console.WriteLine($"Current state: {state2}");
            }
        }
        else
        {
            Console.WriteLine($"Current state: {state1}");
        }
    }
}

public class MachineSession
{
    public VendingMachineState CurrentState { get; set; }
    public required VendingMachineData CurrentData { get; set; }
}
