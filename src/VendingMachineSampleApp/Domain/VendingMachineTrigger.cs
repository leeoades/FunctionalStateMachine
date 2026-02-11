namespace VendingMachineSampleApp.Domain;

/// <summary>
/// Base record for all vending machine triggers.
/// Triggers represent events that cause state transitions.
/// </summary>
public abstract record VendingMachineTrigger;

/// <summary>
/// Triggered when a customer selects an item from the vending machine.
/// The price is looked up from inventory based on the item code.
/// </summary>
public record SelectItemTrigger(string ItemCode) : VendingMachineTrigger;

/// <summary>
/// Triggered when a customer inserts money into the vending machine.
/// </summary>
public record InsertMoneyTrigger(decimal Amount) : VendingMachineTrigger;

/// <summary>
/// Triggered when a customer cancels their current transaction and wants to exit.
/// </summary>
public record CancelTrigger : VendingMachineTrigger;

/// <summary>
/// Triggered when the user asks to see the inventory/help menu.
/// </summary>
public record ShowInventoryTrigger : VendingMachineTrigger;

/// <summary>
/// Triggered when the user requests to exit the application.
/// </summary>
public record ExitTrigger : VendingMachineTrigger;

/// <summary>
/// Triggered when user input does not map to any valid command.
/// </summary>
public record InvalidInputTrigger(string Input) : VendingMachineTrigger;

/// <summary>
/// Triggered when the machine detects a mechanical jam that prevents operation.
/// </summary>
public record JamDetectedTrigger : VendingMachineTrigger;
