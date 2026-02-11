namespace VendingMachineSampleApp.Domain;

/// <summary>
/// Represents the different states a vending machine can be in during operation.
/// </summary>
public enum VendingMachineState
{
    /// <summary>Top-level operational state (parent for all operational states)</summary>
    Operational,

    /// <summary>Waiting for customer to select an item</summary>
    Idle,

    /// <summary>Payment phase (parent state for payment sub-states)</summary>
    Payment,

    /// <summary>Awaiting payment with amount due displayed</summary>
    PaymentMoneyDue,

    /// <summary>Refunding overpayment before completing payment</summary>
    PaymentRefund,

    /// <summary>Payment accepted and ready to dispense item</summary>
    PaymentComplete,

    /// <summary>Dispensing the selected item to the customer</summary>
    DispensingItem,

    /// <summary>Returning change to the customer after successful purchase</summary>
    TransactionComplete,
    
    /// <summary>Machine has encountered a physical jam and cannot operate</summary>
    MachineJammed
}
