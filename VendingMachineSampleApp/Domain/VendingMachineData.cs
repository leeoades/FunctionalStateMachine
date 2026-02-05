namespace VendingMachineSampleApp.Domain;

/// <summary>
/// Represents the data state of the vending machine during operation.
/// This data is carried through all state transitions and can be modified by transition actions.
/// </summary>
/// <param name="Inventory">Dictionary of all items available in the machine, indexed by item code</param>
/// <param name="MoneyInserted">Total amount of money the customer has inserted for current transaction</param>
/// <param name="SelectedItemCode">Item code selected by customer (null when no selection)</param>
/// <param name="DispenseAttempts">Counter for how many times we've attempted to dispense current item</param>
/// <param name="LastTransactionTime">Timestamp of the most recent transaction</param>
/// <param name="TotalRevenue">Cumulative revenue since machine started</param>
public record VendingMachineData(
    Dictionary<string, VendingItem> Inventory,
    decimal MoneyInserted,
    string? SelectedItemCode,
    int DispenseAttempts,
    DateTime LastTransactionTime,
    decimal TotalRevenue)
{
    /// <summary>
    /// Factory method to create initial vending machine data with empty transaction state.
    /// </summary>
    /// <param name="inventory">Pre-populated inventory of items</param>
    public static VendingMachineData Initialize(Dictionary<string, VendingItem> inventory) =>
        new(
            Inventory: inventory,
            MoneyInserted: 0m,
            SelectedItemCode: null,
            DispenseAttempts: 0,
            LastTransactionTime: DateTime.UtcNow,
            TotalRevenue: 0m);

    /// <summary>
    /// Helper property to get the price of the currently selected item.
    /// Returns null if no item is selected or item doesn't exist.
    /// </summary>
    public decimal? SelectedItemPrice =>
        SelectedItemCode != null && Inventory.TryGetValue(SelectedItemCode, out var item)
            ? item.Price
            : null;

    /// <summary>
    /// Helper property to get the name of the currently selected item.
    /// Returns null if no item is selected or item doesn't exist.
    /// </summary>
    public string? SelectedItemName =>
        SelectedItemCode != null && Inventory.TryGetValue(SelectedItemCode, out var item)
            ? item.Name
            : null;

    /// <summary>
    /// Helper property to check if the current selection is in stock.
    /// Returns false if no item is selected.
    /// </summary>
    public bool IsSelectedItemInStock =>
        SelectedItemCode != null && 
        Inventory.TryGetValue(SelectedItemCode, out var item) && 
        item.Quantity > 0;

    /// <summary>
    /// Helper property to calculate how much more money is needed for the selected item.
    /// Returns 0 if payment is sufficient or no item selected.
    /// </summary>
    public decimal AmountStillNeeded =>
        SelectedItemPrice.HasValue 
            ? Math.Max(0, SelectedItemPrice.Value - MoneyInserted)
            : 0m;

    /// <summary>
    /// Helper property to calculate change to return after purchase.
    /// Returns 0 if exact payment or insufficient payment.
    /// </summary>
    public decimal ChangeToReturn =>
        SelectedItemPrice.HasValue
            ? Math.Max(0, MoneyInserted - SelectedItemPrice.Value)
            : 0m;
}
