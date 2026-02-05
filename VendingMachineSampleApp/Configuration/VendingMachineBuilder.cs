using FunctionalStateMachine.Core;
using FunctionalStateMachine.Diagrams;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.Configuration;

/// <summary>
/// Factory class responsible for building and configuring the vending machine state machine.
/// Demonstrates the functional state machine pattern where data is passed through transitions,
/// not stored internally in the state machine.
/// 
/// The state machine is generic over 4 types:
/// - TState: VendingMachineState (the states)
/// - TTrigger: VendingMachineTrigger (the triggers)
/// - TData: VendingMachineData (the mutable data passed through)
/// - TCommand: VendingMachineCommand (commands produced by transitions)
/// </summary>
public static class VendingMachineBuilder
{
    /// <summary>
    /// Builds the complete vending machine state machine with all states, transitions, guards, and commands.
    /// 
    /// State Flow:
    /// Idle → ItemSelected → PaymentValidation → DispensingItem → ReturningChange → Idle
    /// With error paths to OutOfStock and MachineJammed
    /// </summary>
    [StateMachineDiagram("diagrams/VendingMachine.md")]
    public static StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand> BuildMachine()
    {
        return StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand>.Create()
            .StartWith(VendingMachineState.Idle)

            // ============ IDLE STATE ============
            // Waiting for customer to select an item
            .For(VendingMachineState.Idle)
                .On<SelectItemTrigger>()
                    .If((data, trigger) =>
                        data.Inventory.TryGetValue(trigger.ItemCode, out var item) && item.Quantity > 0)
                        .ModifyData((data, trigger) => data with
                        {
                            SelectedItemCode = trigger.ItemCode,
                            DispenseAttempts = 0,
                            MoneyInserted = 0m
                        })
                        .Execute(() => new VendingMachineCommand[]
                        {
                            new PlaySoundCommand(VendingSound.SelectionConfirmed)
                        })
                        .TransitionTo(VendingMachineState.ItemSelected)
                    .ElseIf((data, trigger) => data.Inventory.ContainsKey(trigger.ItemCode))
                        .Execute(() => new VendingMachineCommand[]
                        {
                            new PlaySoundCommand(VendingSound.ErrorSound),
                            new DisplayMessageCommand("Item out of stock. Please select another item.")
                        })
                        .TransitionTo(VendingMachineState.OutOfStock)
                    .Else()
                        .Execute(() => new VendingMachineCommand[]
                        {
                            new PlaySoundCommand(VendingSound.ErrorSound),
                            new DisplayMessageCommand("Invalid item code. Please try again.")
                        })
                    .Done()
                .On<JamDetectedTrigger>()
                    .TransitionTo(VendingMachineState.MachineJammed)

            // ============ ITEM SELECTED STATE ============
            // Customer has selected an item, waiting for payment
            .For(VendingMachineState.ItemSelected)
                .OnEntry(data => new VendingMachineCommand[]
                {
                    new DisplayMessageCommand($"Item selected: {data.SelectedItemName}"),
                    new DisplayMessageCommand($"Price: ${data.SelectedItemPrice:F2}")
                })
                .On<InsertMoneyTrigger>()
                    .ModifyData((data, trigger) => data with { MoneyInserted = data.MoneyInserted + trigger.Amount })
                    .If(data => data.SelectedItemPrice.HasValue && data.MoneyInserted >= data.SelectedItemPrice)
                        .TransitionTo(VendingMachineState.DispensingItem)
                    .Else()
                        .TransitionTo(VendingMachineState.PaymentValidation)
                        .Done()
                .On<CancelTrigger>()
                    .Execute(data => new VendingMachineCommand[]
                    {
                        new DisplayMessageCommand("Transaction cancelled."),
                        new ReturnMoneyCommand(data.MoneyInserted)
                    })
                    .ModifyData(data => data with { MoneyInserted = 0m, SelectedItemCode = null, DispenseAttempts = 0 })
                    .TransitionTo(VendingMachineState.Idle)
                .On<JamDetectedTrigger>()
                    .TransitionTo(VendingMachineState.MachineJammed)

            // ============ PAYMENT VALIDATION STATE ============
            // Checking if customer has paid enough for selected item
            .For(VendingMachineState.PaymentValidation)
                .OnEntry(data => new VendingMachineCommand[]
                {
                    new DisplayMessageCommand(
                        $"Inserted ${data.MoneyInserted:F2}. Remaining ${data.AmountStillNeeded:F2}.")
                })
                .On<InsertMoneyTrigger>()
                    .ModifyData((data, trigger) => data with { MoneyInserted = data.MoneyInserted + trigger.Amount })
                    .If(data => data.SelectedItemPrice.HasValue && data.MoneyInserted >= data.SelectedItemPrice)
                        .TransitionTo(VendingMachineState.DispensingItem)
                    .Else()
                        .Execute(data => new VendingMachineCommand[]
                        {
                            new DisplayMessageCommand(
                                $"Please insert ${data.AmountStillNeeded:F2} more.")
                        })
                        .Done()
                .On<CancelTrigger>()
                    .Execute(data => new VendingMachineCommand[]
                    {
                        new DisplayMessageCommand("Transaction cancelled."),
                        new ReturnMoneyCommand(data.MoneyInserted)
                    })
                    .ModifyData(data => data with { MoneyInserted = 0m, SelectedItemCode = null, DispenseAttempts = 0 })
                    .TransitionTo(VendingMachineState.Idle)
                .On<JamDetectedTrigger>()
                    .TransitionTo(VendingMachineState.MachineJammed)

            // ============ DISPENSING ITEM STATE ============
            // Physically dispensing the selected item
            .For(VendingMachineState.DispensingItem)
                .OnEntry(data => new VendingMachineCommand[]
                {
                    new DisplayMessageCommand($"Dispensing {data.SelectedItemName}..."),
                    new PlaySoundCommand(VendingSound.DispensingSound),
                    new DispenseItemCommand(data.SelectedItemCode!),
                    new UpdateInventoryCommand(data.SelectedItemCode!, 1)
                })
                .On<DispenseCompleteTrigger>()
                    .ModifyData(data => data with
                    {
                        LastTransactionTime = DateTime.UtcNow,
                        TotalRevenue = data.TotalRevenue + (data.SelectedItemPrice ?? 0m)
                    })
                    .If(data => data.ChangeToReturn > 0m)
                        .TransitionTo(VendingMachineState.ReturningChange)
                    .Else()
                        .ModifyData(data => data with { MoneyInserted = 0m, SelectedItemCode = null, DispenseAttempts = 0 })
                        .Execute(() => new VendingMachineCommand[]
                        {
                            new DisplayMessageCommand("Ready for next customer...")
                        })
                        .TransitionTo(VendingMachineState.Idle)
                        .Done()
                .On<JamDetectedTrigger>()
                    .TransitionTo(VendingMachineState.MachineJammed)

            // ============ RETURNING CHANGE STATE ============
            // Returning any change to the customer
            .For(VendingMachineState.ReturningChange)
                .OnEntry(data =>
                {
                    var commands = new List<VendingMachineCommand>
                    {
                        new DisplayMessageCommand("Thank you for your purchase!"),
                        new PlaySoundCommand(VendingSound.TransactionComplete),
                        new LogTransactionCommand(data.SelectedItemCode ?? string.Empty, data.MoneyInserted, true),
                        new UpdateSalesMetricsCommand(data.SelectedItemPrice ?? 0m, true)
                    };

                    if (data.ChangeToReturn > 0)
                    {
                        commands.Add(new ReturnMoneyCommand(data.ChangeToReturn));
                        commands.Add(new PlaySoundCommand(VendingSound.CoinReturn));
                    }

                    return commands.ToArray();
                })
                .On<DispenseCompleteTrigger>()
                    .ModifyData(data => data with
                    {
                        MoneyInserted = 0m,
                        SelectedItemCode = null,
                        DispenseAttempts = 0
                    })
                    .Execute(() => new VendingMachineCommand[]
                    {
                        new DisplayMessageCommand("Ready for next customer...")
                    })
                    .TransitionTo(VendingMachineState.Idle)
                .On<JamDetectedTrigger>()
                    .TransitionTo(VendingMachineState.MachineJammed)

            // ============ OUT OF STOCK STATE ============
            // Item selected is not available
            .For(VendingMachineState.OutOfStock)
                .OnEntry(_ => new VendingMachineCommand[]
                {
                    new UpdateSalesMetricsCommand(0m, false)
                })
                .On<SelectItemTrigger>()
                    .If((data, trigger) =>
                        data.Inventory.TryGetValue(trigger.ItemCode, out var item) && item.Quantity > 0)
                        .ModifyData((data, trigger) => data with
                        {
                            SelectedItemCode = trigger.ItemCode,
                            DispenseAttempts = 0,
                            MoneyInserted = 0m
                        })
                        .Execute(() => new VendingMachineCommand[]
                        {
                            new PlaySoundCommand(VendingSound.SelectionConfirmed)
                        })
                        .TransitionTo(VendingMachineState.ItemSelected)
                    .ElseIf((data, trigger) => data.Inventory.ContainsKey(trigger.ItemCode))
                        .Execute(() => new VendingMachineCommand[]
                        {
                            new DisplayMessageCommand("Item out of stock. Please select another item.")
                        })
                        .Done()
                .On<SelectItemTrigger>()
                    .Execute(() => new VendingMachineCommand[]
                    {
                        new DisplayMessageCommand("Invalid item code. Please try again.")
                    })
                .On<CancelTrigger>()
                    .ModifyData(data => data with { MoneyInserted = 0m, SelectedItemCode = null, DispenseAttempts = 0 })
                    .TransitionTo(VendingMachineState.Idle)
                .On<JamDetectedTrigger>()
                    .TransitionTo(VendingMachineState.MachineJammed)

            // ============ MACHINE JAMMED STATE ============
            // Machine has encountered a mechanical error
            .For(VendingMachineState.MachineJammed)
                .OnEntry(data => new VendingMachineCommand[]
                {
                    new PlaySoundCommand(VendingSound.JamAlert),
                    new DisplayMessageCommand("Machine error detected. Service required."),
                    new ReturnMoneyCommand(data.MoneyInserted)
                })

            .Build();
    }
}
