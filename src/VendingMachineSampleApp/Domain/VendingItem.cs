namespace VendingMachineSampleApp.Domain;

/// <summary>
/// Represents a single item available in the vending machine.
/// </summary>
/// <param name="Code">Unique identifier for the item (e.g., "A1", "B2")</param>
/// <param name="Name">Display name of the item (e.g., "Chips", "Soda")</param>
/// <param name="Price">Price of the item in dollars</param>
/// <param name="Quantity">Current quantity in stock</param>
public record VendingItem(string Code, string Name, decimal Price, int Quantity);
