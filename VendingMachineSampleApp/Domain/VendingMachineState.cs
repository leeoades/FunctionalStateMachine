namespace VendingMachineSampleApp.Domain;

/// <summary>
/// Represents the different states a vending machine can be in during operation.
/// </summary>
public enum VendingMachineState
{
    /// <summary>Waiting for customer to select an item</summary>
    Idle,

    /// <summary>Customer has selected an item, waiting for payment validation</summary>
    ItemSelected,

    /// <summary>Validating if customer has paid enough for the selected item</summary>
    PaymentValidation,

    /// <summary>Dispensing the selected item to the customer</summary>
    DispensingItem,

    /// <summary>Returning change to the customer after successful purchase</summary>
    ReturningChange,

    /// <summary>Requested item is out of stock</summary>
    OutOfStock,

    /// <summary>Machine has encountered a physical jam and cannot operate</summary>
    MachineJammed
}
