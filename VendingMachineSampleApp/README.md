# Vending Machine State Machine Sample

A complete, interactive vending machine simulator demonstrating the FunctionalStateMachine library with dependency injection, multiple command handlers, and complex state transitions.

## Overview

This sample showcases:

- ✅ **Complex State Machine** - Hierarchical states with payment sub-flows
- ✅ **Multiple Command Types** - Command handlers with distinct concerns
- ✅ **Dependency Injection** - Automatic handler scanning and registration
- ✅ **State Data Mutation** - Tracking money, inventory, and metrics throughout transactions
- ✅ **Real-World Domain** - Domain-driven design with meaningful guards and business logic
- ✅ **Interactive Console UI** - Run live transactions and see all system components in action

## State Machine Architecture

```
                         ┌──────────────────────┐
                         │    IDLE (start)      │
                         └──────────┬───────────┘
                                    │
                         SelectItem (item in stock?)
                                    │
                     ┌──────────────▼───────────────┐
                     │      PAYMENT (superstate)    │
                     └──────────────┬───────────────┘
                                    │
                             MONEY DUE (amount due)
                                    │
                        InsertMoney (recalculate due)
                      ┌─────────────┴─────────────┐
                      │                           │
                due remains                   overpaid
                      │                           │
                MONEY DUE                   PAYMENT REFUND
                                                  │
                                           refund issued
                                                  │
                                            PAYMENT COMPLETE
                                                  │
                                            DISPENSING ITEM
                                                  │
                                            RETURNING CHANGE
                                                  │
                                               IDLE

ERROR PATHS:
- SelectItem → OUT OF STOCK (item exists but Quantity=0)
- SelectItem → IDLE (invalid item code)
- Any State → MACHINE JAMMED (jam detected trigger)
- Payment/OutOfStock → IDLE (cancel trigger)
```

## Domain Model

### States
- **Operational** - Top-level superstate for common transitions
- **Idle** - Waiting for customer to select an item
- **Payment** - Superstate for payment phase
- **PaymentMoneyDue** - Displays amount due, waits for money
- **PaymentRefund** - Issues refund for overpayment
- **PaymentComplete** - Payment accepted; ready to dispense
- **DispensingItem** - Physically dispensing the selected item
- **ReturningChange** - Returning change to customer
- **OutOfStock** - Item selected is not available
- **MachineJammed** - Machine encountered a mechanical error

### Triggers
- `SelectItemTrigger(itemCode)` - Customer selects an item
- `InsertMoneyTrigger(amount)` - Customer inserts money
- `ShowInventoryTrigger` - User requests the inventory menu
- `ExitTrigger` - User requests to exit the application
- `InvalidInputTrigger(input)` - User input didn't map to a command
- `CancelTrigger` - Customer cancels the transaction
- `JamDetectedTrigger` - Machine detects a jam

### Commands (Dispatched by Transitions/Entry)
1. **DisplayMessageCommand** → DisplayMessageHandler
   - Shows messages on machine display
   
2. **LogTransactionCommand** → LogTransactionHandler
   - Records transactions to persistent storage
   
3. **DispenseItemCommand** → DispenseItemHandler
   - Actuates motor/mechanism to dispense product
   
4. **ReturnMoneyCommand** → ReturnMoneyHandler
   - Returns coins/bills to customer
   
5. **UpdateInventoryCommand** → UpdateInventoryHandler
   - Decrements stock after successful dispensing
   
6. **PlaySoundCommand** → PlaySoundHandler
   - Plays beeps, chimes, error sounds
   
7. **UpdateSalesMetricsCommand** → UpdateSalesMetricsHandler
   - Tracks revenue and transaction success rate
8. **ShowInventoryCommand** → ShowInventoryHandler
   - Renders inventory and menu options
9. **ExitApplicationCommand** → ExitApplicationHandler
   - Signals the host to exit after commands are executed

## Key Features

### Guards
```csharp
// Item must exist and be in stock
.If((data, trigger) =>
    data.Inventory.TryGetValue(trigger.ItemCode, out var item) &&
    item.Quantity > 0)
    .TransitionTo(VendingMachineState.Payment)

// Must have paid enough
.If(data => data.SelectedItemPrice.HasValue &&
            data.MoneyInserted >= data.SelectedItemPrice)
    .TransitionTo(VendingMachineState.PaymentComplete)
```

### State Data Mutation
```csharp
.ModifyData((data, trigger) => data with
{
    SelectedItemCode = trigger.ItemCode,
    MoneyInserted = 0m,
    DispenseAttempts = 0
})
```

### Entry Actions
```csharp
.For(VendingMachineState.DispensingItem)
    .OnEntry(data => new VendingMachineCommand[]
    {
        new DisplayMessageCommand($"Dispensing {data.SelectedItemName}..."),
        new PlaySoundCommand(VendingSound.DispensingSound),
        new DispenseItemCommand(data.SelectedItemCode!),
        new UpdateInventoryCommand(data.SelectedItemCode!, 1)
    })
```

## Running the Sample

```bash
dotnet run --project VendingMachineSampleApp\VendingMachineSampleApp.csproj
```

## Interactive Commands

Once running, the machine awaits customer interaction:

```
Type HELP to see items or EXIT to quit.

Enter command:
```

### Example Interaction

```
Enter command: B1
🔊 *beep*
🖥️  Item selected: Soda
🖥️  Price: $2.00
Current state: PaymentMoneyDue

Enter command: 1.50
🖥️  Inserted $1.50. Remaining $0.50.
Current state: PaymentMoneyDue

Enter command: 0.50
🖥️  Dispensing Soda...
🔊 *whirrrr*
🔊 *motor whirring sounds*
📦 Dispensing B1...
✓ Item dispensed successfully
📊 [INVENTORY] Soda: 3 → 2
🖥️  Thank you for your purchase!
🔊 *ding!*
📈 [METRICS] Revenue: $2.00 | Success Rate: 100.0% | Successful: 1 | Failed: 0

🖥️  Ready for next customer...
Current state: Idle
```

## Project Structure

```
VendingMachine/
├── Domain/
│   ├── VendingMachineState.cs      # State enum
│   ├── VendingMachineTrigger.cs    # Trigger types
│   ├── VendingMachineCommand.cs    # Command types
│   ├── VendingItem.cs              # Inventory item
│   └── VendingMachineData.cs       # Machine state data
├── CommandRunners/
│   ├── DisplayMessageHandler.cs
│   ├── LogTransactionHandler.cs
│   ├── DispenseItemHandler.cs
│   ├── ReturnMoneyHandler.cs
│   ├── UpdateInventoryHandler.cs
│   ├── PlaySoundHandler.cs
│   ├── UpdateSalesMetricsHandler.cs
│   ├── ShowInventoryHandler.cs
│   └── ExitApplicationHandler.cs
├── Configuration/
│   └── VendingMachineBuilder.cs    # State machine definition
└── VendingMachineSample.cs         # Interactive console app
```

## What This Demonstrates

### For State Machine Users
- How to build complex state machines with guards and guards composition
- Entry/exit actions that dispatch multiple command types
- State data mutation through transitions
- Real-world domain modeling

### For Library Features
- ✅ **If/ElseIf/Else Guards** - Multiple conditional paths from one trigger
- ✅ **Complex Data State** - Records with helper properties
- ✅ **Entry Actions with Commands** - Automatic command generation on state entry
- ✅ **State Mutation** - Data flowing through transitions with transformations
- ✅ **Multiple Guard Types** - Different predicates for different paths
- ✅ **Error Handling** - Invalid selections, out of stock, etc.

### For Dependency Injection Patterns
- How to register multiple handler implementations
- Service provider integration with command dispatcher
- Handlers with dependencies (inventory for UpdateInventoryHandler)

## Notes

- Each user input maps to a trigger; the state machine decides if it is valid
- Commands are dispatched **immediately after** state transitions
- State data is immutable - transitions return new data instances
- Guards use short-circuit evaluation - first matching condition wins
- Entry actions execute when state is entered, regardless of how
- The interactive session demonstrates both valid flows and error paths

## Future Enhancements

- Add timeout transitions (auto-return money after inactivity)
- History pseudostates (remember last selected item)
- Persistent transaction logging to file
- Network integration (remote vending machine reporting)
- State machine diagram generation
