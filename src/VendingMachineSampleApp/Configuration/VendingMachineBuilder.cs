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
    /// Idle → Payment (MoneyDue → Refund? → Complete) → DispensingItem → ReturningChange → Idle
    /// With error pathS to MachineJammed
    /// </summary>
    [StateMachineDiagram("diagrams/VendingMachine.md")]
    public static StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand>
        BuildMachine()
    {
        return StateMachine<VendingMachineState, VendingMachineTrigger, VendingMachineData, VendingMachineCommand>
            .Create()
            .StartWith(VendingMachineState.Operational)

            // ============ OPERATIONAL SUPERSTATE ============
            .For(VendingMachineState.Operational)
            .StartsWith(VendingMachineState.Idle)
            .On<ShowInventoryTrigger>()
            .Execute(() => [new ShowInventoryCommand()])
            .Done()
            .On<ExitTrigger>()
            .Execute(() => [new ExitApplicationCommand()])
            .Done()
            .On<InvalidInputTrigger>()
            .Execute((_, trigger) =>
            [
                new DisplayMessageCommand($"Invalid input: \"{trigger.Input}\". Type HELP for options.")
            ])
            .Done()
            .On<CancelTrigger>()
            .Execute(data =>
            [
                new DisplayMessageCommand("Transaction cancelled."),
                new ReturnMoneyCommand(data.MoneyInserted)
            ])
            .ModifyData(data => data with { MoneyInserted = 0m, SelectedItemCode = null, DispenseAttempts = 0 })
            .TransitionTo(VendingMachineState.Idle)
            .On<InsertMoneyTrigger>()
            .Execute(() =>
            [
                new DisplayMessageCommand("Please select an item before inserting money.")
            ])
            .Done()
            .On<JamDetectedTrigger>()
            .TransitionTo(VendingMachineState.MachineJammed)

            // ============ IDLE STATE ============
            // Waiting for customer to select an item
            .For(VendingMachineState.Idle)
            .SubStateOf(VendingMachineState.Operational)
            .OnEntry(() => new ShowInventoryCommand())
            .On<SelectItemTrigger>()
            .If((data, trigger) =>
                data.Inventory.TryGetValue(trigger.ItemCode, out var item) && item.Quantity > 0)
            .ModifyData((data, trigger) => data with
            {
                SelectedItemCode = trigger.ItemCode,
                DispenseAttempts = 0,
                MoneyInserted = 0m
            })
            .Execute(() =>
            [
                new PlaySoundCommand(VendingSound.SelectionConfirmed)
            ])
            .TransitionTo(VendingMachineState.Payment)
            .ElseIf((data, trigger) => data.Inventory.ContainsKey(trigger.ItemCode))
            .Execute(() =>
            [
                new PlaySoundCommand(VendingSound.ErrorSound),
                new DisplayMessageCommand("Item out of stock. Please select another item.")
            ])
            .Else()
            .Execute(() =>
            [
                new PlaySoundCommand(VendingSound.ErrorSound),
                new DisplayMessageCommand("Invalid item code. Please try again.")
            ])
            .Done()

            // ============ PAYMENT SUPERSTATE ============
            .For(VendingMachineState.Payment)
            .SubStateOf(VendingMachineState.Operational)
            .StartsWith(VendingMachineState.PaymentMoneyDue)

            // ============ PAYMENT MONEY DUE ============
            .For(VendingMachineState.PaymentMoneyDue)
            .SubStateOf(VendingMachineState.Payment)
            .OnEntry(data =>
            [
                new DisplayMessageCommand($"Please insert coins. Amount due: £{data.AmountStillNeeded:F2}.")
            ])
            .On<InsertMoneyTrigger>()
            .ModifyData((data, trigger) => data with { MoneyInserted = data.MoneyInserted + trigger.Amount })
            .If(data => data.SelectedItemPrice.HasValue && data.MoneyInserted > data.SelectedItemPrice)
            .TransitionTo(VendingMachineState.PaymentRefund)
            .ElseIf(data => data.SelectedItemPrice.HasValue && data.MoneyInserted == data.SelectedItemPrice)
            .TransitionTo(VendingMachineState.PaymentComplete)
            .Else()
            .Execute(data =>
            [
                new DisplayMessageCommand($"Please insert coins. Amount due: £{data.AmountStillNeeded:F2}.")
            ])
            .TransitionTo(VendingMachineState.PaymentMoneyDue)
            .Done()

            // ============ PAYMENT REFUND ============
            .For(VendingMachineState.PaymentRefund)
            .SubStateOf(VendingMachineState.Payment)
            .OnEntry(data =>
            [
                new DisplayMessageCommand($"Overpaid by £{data.ChangeToReturn:F2}. Issuing refund."),
                new ReturnMoneyCommand(data.ChangeToReturn)
            ])
            .Immediately()
            .ModifyData(data => data with { MoneyInserted = data.SelectedItemPrice ?? data.MoneyInserted })
            .TransitionTo(VendingMachineState.PaymentComplete)
            .Done()

            // ============ PAYMENT COMPLETE ============
            .For(VendingMachineState.PaymentComplete)
            .SubStateOf(VendingMachineState.Payment)
            .OnEntry(_ =>
            [
                new DisplayMessageCommand("Payment complete.")
            ])
            .Immediately()
            .TransitionTo(VendingMachineState.DispensingItem)
            .Done()

            // ============ DISPENSING ITEM STATE ============
            // Physically dispensing the selected item
            .For(VendingMachineState.DispensingItem)
            .SubStateOf(VendingMachineState.Operational)
            .OnEntry(data =>
            [
                new DisplayMessageCommand($"Dispensing {data.SelectedItemName}..."),
                new PlaySoundCommand(VendingSound.DispensingSound),
                new DispenseItemCommand(data.SelectedItemCode!),
                new UpdateInventoryCommand(data.SelectedItemCode!, 1)
            ])
            .Immediately()
            .ModifyData(data => data with
            {
                LastTransactionTime = DateTime.UtcNow,
                TotalRevenue = data.TotalRevenue + (data.SelectedItemPrice ?? 0m)
            })
            .TransitionTo(VendingMachineState.TransactionComplete)
            .Done()

            // ============ TRANSACTION COMPLETE STATE ============
            .For(VendingMachineState.TransactionComplete)
            .SubStateOf(VendingMachineState.Operational)
            .OnEntry(data =>
            [
                new DisplayMessageCommand("Thank you for your purchase!"),
                new PlaySoundCommand(VendingSound.TransactionComplete),
                new LogTransactionCommand(data.SelectedItemCode ?? string.Empty, data.MoneyInserted, true),
                new UpdateSalesMetricsCommand(data.SelectedItemPrice ?? 0m, true)
            ])
            .Immediately()
            .ModifyData(data => data with
            {
                MoneyInserted = 0m,
                SelectedItemCode = null,
                DispenseAttempts = 0
            })
            .Execute(() =>
            [
                new DisplayMessageCommand("Ready for next customer...")
            ])
            .TransitionTo(VendingMachineState.Idle)
            .Done()
            
            // ============ MACHINE JAMMED STATE ============
            // Machine has encountered a mechanical error
            .For(VendingMachineState.MachineJammed)
            .SubStateOf(VendingMachineState.Operational)
            .OnEntry(data =>
            [
                new PlaySoundCommand(VendingSound.JamAlert),
                new DisplayMessageCommand("Machine error detected. Service required."),
                new ReturnMoneyCommand(data.MoneyInserted)
            ])
            .Build();
    }
}
