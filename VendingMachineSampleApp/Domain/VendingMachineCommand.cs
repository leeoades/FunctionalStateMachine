namespace VendingMachineSampleApp.Domain;

/// <summary>
/// Base abstract record for all vending machine commands.
/// Commands represent work that should be executed as a result of state transitions.
/// Each command type will have a corresponding handler in the CommandRunners folder.
/// </summary>
public abstract record VendingMachineCommand;

/// <summary>
/// Command to display a message to the customer on the vending machine display.
/// Example: "Please insert $1.50 more"
/// </summary>
public record DisplayMessageCommand(string Message) : VendingMachineCommand;

/// <summary>
/// Command to log a transaction to persistent storage.
/// Records what item was purchased, how much was paid, and whether it was successful.
/// </summary>
public record LogTransactionCommand(string ItemCode, decimal AmountPaid, bool Success) : VendingMachineCommand;

/// <summary>
/// Command to physically dispense an item from the vending machine.
/// This simulates actuating the motor/mechanism to deliver the product.
/// </summary>
public record DispenseItemCommand(string ItemCode) : VendingMachineCommand;

/// <summary>
/// Command to return money to the customer via the coin return slot.
/// This simulates dispensing physical coins/bills.
/// </summary>
public record ReturnMoneyCommand(decimal Amount) : VendingMachineCommand;

/// <summary>
/// Command to update the inventory after an item is successfully dispensed.
/// Decrements the quantity available for that item.
/// </summary>
public record UpdateInventoryCommand(string ItemCode, int QuantityDispensed) : VendingMachineCommand;

/// <summary>
/// Command to play a sound effect on the vending machine.
/// Enum specifies which sound to play (beep, chime, error sound, etc.)
/// </summary>
public record PlaySoundCommand(VendingSound Sound) : VendingMachineCommand;

/// <summary>
/// Command to update sales metrics/analytics.
/// Tracks daily revenue and transaction success rate for reporting.
/// </summary>
public record UpdateSalesMetricsCommand(decimal Revenue, bool Success) : VendingMachineCommand;

/// <summary>
/// Enumeration of sound effects the vending machine can play.
/// </summary>
public enum VendingSound
{
    /// <summary>Generic beep sound for confirmation</summary>
    SelectionConfirmed,

    /// <summary>Ding sound when transaction completes successfully</summary>
    TransactionComplete,

    /// <summary>Error buzz when transaction fails</summary>
    ErrorSound,

    /// <summary>Mechanical whirring sound when dispensing item</summary>
    DispensingSound,

    /// <summary>Coin return chime when returning change</summary>
    CoinReturn,

    /// <summary>Alert sound when machine detects jam</summary>
    JamAlert
}
