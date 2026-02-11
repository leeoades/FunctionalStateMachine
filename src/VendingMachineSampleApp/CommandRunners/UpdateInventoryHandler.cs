using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles UpdateInventoryCommand by decrementing the stock of a dispensed item.
/// Maintains the in-memory inventory state as items are sold.
/// </summary>
public class UpdateInventoryHandler : ICommandRunner<UpdateInventoryCommand>
{
    private readonly Dictionary<string, VendingItem> _inventory;

    public UpdateInventoryHandler(Dictionary<string, VendingItem> inventory)
    {
        _inventory = inventory;
    }

    public void Run(UpdateInventoryCommand command)
    {
        if (_inventory.TryGetValue(command.ItemCode, out var item))
        {
            var newQuantity = item.Quantity - command.QuantityDispensed;
            _inventory[command.ItemCode] = item with { Quantity = newQuantity };
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"📊 [INVENTORY] {item.Name}: {item.Quantity} → {newQuantity}");
            Console.ResetColor();
        }
    }
}
