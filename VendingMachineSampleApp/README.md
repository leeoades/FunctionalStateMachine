# Vending Machine State Machine Sample

A complete, interactive vending machine simulator demonstrating the FunctionalStateMachine library with dependency injection, multiple command handlers, and complex state transitions.

## Overview

This sample showcases:

- ✅ **Complex State Machine** - 7 states with sophisticated guards and transitions
- ✅ **Multiple Command Types** - 7 different command handlers with distinct concerns
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
                     │    ITEM SELECTED             │
                     │ (item code validated)        │
                     └─────────────┬────────────────┘
                                   │
                       InsertMoney (first payment)
                                   │
            ┌──────────────────────▼───────────────────────┐
            │  PAYMENT VALIDATION                          │
            │  (check: MoneyInserted >= ItemPrice?)        │
            └────────┬──────────────────────────────────┬──┘
                     │                                  │
        InsertMoney  │ (insufficient)      InsertMoney  │ (sufficient)
                     │                                  │
                     └─────────────┬────────────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │  DISPENSING ITEM            │
                    │ (motor control simulation)  │
                    └────────────┬────────────────┘
                                 │
                         DispenseComplete
                                 │
                    ┌───────────┬▼───────────┐
                    │           │            │
         (if change>0)    (return change)  (no change)
                    │           │            │
                    ▼           ▼            │
                 ┌──────────────────┐        │
                 │ RETURNING CHANGE │────────┘
                 │ (coin mechanism) │
                 └────────┬─────────┘
                          │
                   DispenseComplete
                          │
                    ┌─────▼────────┐
                    │    IDLE      │ (ready for next customer)
                    └──────────────┘

ERROR PATHS:
- SelectItem → OUT OF STOCK (item exists but Quantity=0)
- SelectItem → IDLE (invalid item code)
- Any State → MACHINE JAMMED (jam detected trigger)
- ItemSelected/PaymentValidation → IDLE (cancel trigger)
```

## Domain Model

### States
- **Idle** - Waiting for customer to select an item
- **ItemSelected** - Customer selected item, awaiting payment
- **PaymentValidation** - Checking if payment is sufficient
- **DispensingItem** - Physically dispensing the selected item
- **ReturningChange** - Returning change to customer
- **OutOfStock** - Item selected is not available
- **MachineJammed** - Machine encountered a mechanical error

### Triggers
- `SelectItemTrigger(itemCode)` - Customer selects an item
- `InsertMoneyTrigger(amount)` - Customer inserts money
- `DispenseCompleteTrigger` - Dispensing mechanism completed successfully
- `CancelTrigger` - Customer cancels the transaction
- `JamDetectedTrigger` - Machine detects a jam

### Commands (Dispatched on State Entry/Exit)
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

## Key Features

### Guards
```csharp
// Item must exist and be in stock
.If((data, trigger) =>
    data.Inventory.TryGetValue(trigger.ItemCode, out var item) &&
    item.Quantity > 0)
    .TransitionTo(VendingMachineState.ItemSelected)

// Must have paid enough
.If(data => data.SelectedItemPrice.HasValue &&
            data.MoneyInserted >= data.SelectedItemPrice)
    .TransitionTo(VendingMachineState.DispensingItem)
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

### Entry/Exit Actions
```csharp
.For(VendingMachineState.DispensingItem)
    .OnEntry((data, _) => new VendingMachineCommand[]
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
📦 Available Items:
─────────────────────────────────────
  A1  Chips           $1.50   (5 in stock)
  A2  Candy           $0.75   (10 in stock)
  B1  Soda            $2.00   (3 in stock)
  B2  Water           $1.00   (8 in stock)
  C1  Cookies         $1.25   (4 in stock)
─────────────────────────────────────
  HELP - Show this menu
  EXIT - Quit the program

Enter command:
```

### Example Interaction

```
Enter command: B1
🔊 *beep*
🖥️  Item selected: Soda
🖥️  Price: $2.00
Current state: ItemSelected

Enter amount to insert ($): 1.50

🖥️  Inserted $1.50. Remaining $0.50.
Current state: PaymentValidation

Enter command: 0.50

💳 Processing payment...

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
│   └── UpdateSalesMetricsHandler.cs
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
