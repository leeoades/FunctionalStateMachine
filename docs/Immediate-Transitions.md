# Immediate Transitions

Immediate transitions automatically advance from a state without waiting for a trigger. As soon as the state is entered, the machine evaluates immediate transition guards and moves to the next state if conditions are met.

## Table of Contents

1. [Why Use Immediate Transitions](#why-use-immediate-transitions)
2. [Basic Immediate Transition](#basic-immediate-transition)
3. [Guarded Immediate Transitions](#guarded-immediate-transitions)
4. [Multiple Immediate Transitions](#multiple-immediate-transitions)
5. [Starting the Machine](#starting-the-machine)
6. [Complete Example](#complete-example)

---

## Why Use Immediate Transitions

**No trigger needed** — State automatically advances based on conditions  
**Gateway states** — Create decision points in your workflow  
**Initialization** — Set up states that immediately move to the next logical state  
**Clean design** — Avoid placeholder triggers like "Continue" or "Next"

---

## Basic Immediate Transition

Use `.Immediately()` to define an automatic transition:

```csharp
public enum AppState { Starting, Ready }

public abstract record AppTrigger
{
    public sealed record UserAction : AppTrigger;
}

public abstract record AppCommand
{
    public sealed record LoadConfiguration : AppCommand;
    public sealed record ShowUI : AppCommand;
}

var machine = StateMachine<AppState, AppTrigger, AppCommand>.Create()
    .StartWith(AppState.Starting)
    .For(AppState.Starting)
        .OnEntry(() => new AppCommand.LoadConfiguration())
        .Immediately()                          // Automatic transition
            .TransitionTo(AppState.Ready)
            .Done()                             // Required to close Immediately block
    .For(AppState.Ready)
        .OnEntry(() => new AppCommand.ShowUI())
    .Build();

// Start the machine and run immediate transitions
var (state, commands) = machine.Start();
// state == AppState.Ready (automatically advanced from Starting)
// commands == [
//   LoadConfiguration(),    // OnEntry Starting
//   ShowUI()                // OnEntry Ready (after immediate transition)
// ]
```

**How it works:**
1. Machine starts in `Starting` state
2. Entry commands run for `Starting`
3. Immediate transition evaluates and advances to `Ready`
4. Entry commands run for `Ready`
5. All happens in one call to `.Start()`

**Important:** Use `.Start()` to trigger immediate transitions from the initial state. `.Fire()` skips them.

---

## Guarded Immediate Transitions

Add conditions to control whether the immediate transition happens:

```csharp
public sealed record AppData(bool ConfigLoaded, bool DatabaseReady);

var machine = StateMachine<AppState, AppTrigger, AppData, AppCommand>.Create()
    .StartWith(AppState.Initializing)
    .For(AppState.Initializing)
        .OnEntry(() => new AppCommand.LoadConfiguration())
        .Immediately()
            .Guard(data => data.ConfigLoaded && data.DatabaseReady)  // Condition
            .TransitionTo(AppState.Ready)
            .Done()
    .For(AppState.Ready)
        .OnEntry(() => new AppCommand.ShowUI())
    .Build();

// Scenario 1: Not ready yet
var data1 = new AppData(ConfigLoaded: false, DatabaseReady: false);
var (state1, _, commands1) = machine.Start(data1);
// state1 == AppState.Initializing (stayed in initial state)
// Guard failed, immediate transition didn't happen

// Scenario 2: Ready
var data2 = new AppData(ConfigLoaded: true, DatabaseReady: true);
var (state2, _, commands2) = machine.Start(data2);
// state2 == AppState.Ready (immediately transitioned)
// Guard passed, immediate transition happened
```

**When guard fails:**
- Machine stays in the current state
- No transition occurs
- Application can fire triggers later to move the machine

---

## Multiple Immediate Transitions

Define multiple immediate transitions with different guards (first matching wins):

```csharp
public enum ProcessState { Checking, Approved, Rejected, NeedsReview }

public sealed record ProcessData(int Score, bool IsVIP);

public abstract record ProcessCommand
{
    public sealed record SendApproval : ProcessCommand;
    public sealed record SendRejection : ProcessCommand;
    public sealed record RequestReview : ProcessCommand;
}

var machine = StateMachine<ProcessState, ProcessTrigger, ProcessData, ProcessCommand>.Create()
    .StartWith(ProcessState.Checking)
    .For(ProcessState.Checking)
        .Immediately()
            .Guard(data => data.IsVIP && data.Score >= 50)  // VIP with decent score
            .Execute(() => new ProcessCommand.SendApproval())
            .TransitionTo(ProcessState.Approved)
        .Immediately()
            .Guard(data => !data.IsVIP && data.Score >= 70) // Non-VIP needs higher score
            .Execute(() => new ProcessCommand.SendApproval())
            .TransitionTo(ProcessState.Approved)
        .Immediately()
            .Guard(data => data.Score < 30)                 // Very low score
            .Execute(() => new ProcessCommand.SendRejection())
            .TransitionTo(ProcessState.Rejected)
        .Immediately()  // Catch-all: no guard means "always execute"
            .Execute(() => new ProcessCommand.RequestReview())
            .TransitionTo(ProcessState.NeedsReview)
        .Done()
    .Build();

// Test different scenarios

// Scenario 1: VIP with score 60
var data1 = new ProcessData(Score: 60, IsVIP: true);
var (state1, _, _) = machine.Start(data1);
// state1 == ProcessState.Approved (first guard matched)

// Scenario 2: Non-VIP with score 80
var data2 = new ProcessData(Score: 80, IsVIP: false);
var (state2, _, _) = machine.Start(data2);
// state2 == ProcessState.Approved (second guard matched)

// Scenario 3: Score 20
var data3 = new ProcessData(Score: 20, IsVIP: false);
var (state3, _, _) = machine.Start(data3);
// state3 == ProcessState.Rejected (third guard matched)

// Scenario 4: Score 50, not VIP
var data4 = new ProcessData(Score: 50, IsVIP: false);
var (state4, _, _) = machine.Start(data4);
// state4 == ProcessState.NeedsReview (catch-all guard matched)
```

**Evaluation order:**
- Guards are checked in the order you define them
- First matching guard wins
- Execution stops after first match
- Use `.Guard(data => true)` as catch-all at the end

---

## Chaining Immediate Transitions

Immediate transitions can chain through multiple states:

```csharp
public enum SetupState { Starting, LoadingConfig, CheckingAuth, Ready }

public sealed record SetupData(bool ConfigLoaded, bool Authenticated);

var machine = StateMachine<SetupState, SetupTrigger, SetupData, SetupCommand>.Create()
    .StartWith(SetupState.Starting)
    
    .For(SetupState.Starting)
        .OnEntry(() => new SetupCommand.Log("Starting..."))
        .Immediately()
            .TransitionTo(SetupState.LoadingConfig)
            .Done()
    
    .For(SetupState.LoadingConfig)
        .OnEntry(() => new SetupCommand.LoadConfig())
        .Immediately()
            .Guard(data => data.ConfigLoaded)
            .TransitionTo(SetupState.CheckingAuth)
            .Done()
    
    .For(SetupState.CheckingAuth)
        .OnEntry(() => new SetupCommand.CheckAuth())
        .Immediately()
            .Guard(data => data.Authenticated)
            .TransitionTo(SetupState.Ready)
            .Done()
    
    .For(SetupState.Ready)
        .OnEntry(() => new SetupCommand.Log("Ready!"))
    
    .Build();

// If all conditions met, chains through all states automatically
var data = new SetupData(ConfigLoaded: true, Authenticated: true);
var (state, _, commands) = machine.Start(data);
// state == SetupState.Ready
// Machine automatically went: Starting → LoadingConfig → CheckingAuth → Ready
// commands include all OnEntry commands from all states traversed
```

**Important:** Be careful with chains to avoid infinite loops. Each immediate transition should move closer to a terminal state.

---

## Starting the Machine

### Using Start() vs Fire()

**`.Start(data)` — Triggers immediate transitions**
```csharp
var (state, data, commands) = machine.Start(initialData);
// - Enters initial state
// - Runs OnEntry for initial state
// - Evaluates and executes immediate transitions
// - Returns final state after all immediate transitions
```

**`.Fire(trigger, state, data)` — Does NOT trigger immediate transitions**
```csharp
var (state, data, commands) = machine.Fire(trigger, currentState, currentData);
// - Processes the trigger
// - May transition to new state
// - Does NOT evaluate immediate transitions automatically
// - Use Start() after Fire() if you need immediate transitions
```

### When to use each:

**Use `.Start()`:**
- When initializing the machine for the first time
- After loading persisted state that might have immediate transitions
- When you want immediate transitions to run

**Use `.Fire()`:**
- Normal operation after initialization
- Processing external triggers/events
- When immediate transitions shouldn't run

---

## Complete Example

An order processing system with gateway states:

```csharp
public enum OrderState
{
    Submitted,
    ValidatingInventory,
    InventoryOk,
    InsufficientInventory,
    ValidatingPayment,
    PaymentApproved,
    PaymentDeclined,
    ProcessingShipment,
    Completed
}

public abstract record OrderTrigger
{
    public sealed record InventoryChecked(bool Available) : OrderTrigger;
    public sealed record PaymentProcessed(bool Approved) : OrderTrigger;
    public sealed record Shipped : OrderTrigger;
}

public sealed record OrderData(
    Guid OrderId,
    bool InventoryAvailable,
    bool PaymentApproved,
    decimal Total);

public abstract record OrderCommand
{
    public sealed record CheckInventory(Guid OrderId) : OrderCommand;
    public sealed record ProcessPayment(Guid OrderId, decimal Amount) : OrderCommand;
    public sealed record CancelOrder(Guid OrderId, string Reason) : OrderCommand;
    public sealed record ShipOrder(Guid OrderId) : OrderCommand;
    public sealed record SendConfirmation(Guid OrderId) : OrderCommand;
}

var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .StartWith(OrderState.Submitted)
    
    // Submitted: Gateway state that immediately checks inventory
    .For(OrderState.Submitted)
        .Immediately()
            .TransitionTo(OrderState.ValidatingInventory)
            .Done()
    
    // ValidatingInventory: Trigger inventory check, wait for result
    .For(OrderState.ValidatingInventory)
        .OnEntry(data => new OrderCommand.CheckInventory(data.OrderId))
        .On<OrderTrigger.InventoryChecked>()
            .ModifyData((data, trigger) => data with 
            { 
                InventoryAvailable = trigger.Available 
            })
            .TransitionTo(OrderState.InventoryOk)
    
    // InventoryOk: Gateway that decides next step based on inventory
    .For(OrderState.InventoryOk)
        .Immediately()
            .Guard(data => data.InventoryAvailable)
            .TransitionTo(OrderState.ValidatingPayment)
        .Immediately()
            .Guard(data => !data.InventoryAvailable)
            .Execute(data => new OrderCommand.CancelOrder(data.OrderId, "Out of stock"))
            .TransitionTo(OrderState.InsufficientInventory)
        .Done()
    
    // InsufficientInventory: Terminal state
    .For(OrderState.InsufficientInventory)
        // Order cancelled
    
    // ValidatingPayment: Process payment, wait for result
    .For(OrderState.ValidatingPayment)
        .OnEntry(data => new OrderCommand.ProcessPayment(data.OrderId, data.Total))
        .On<OrderTrigger.PaymentProcessed>()
            .ModifyData((data, trigger) => data with 
            { 
                PaymentApproved = trigger.Approved 
            })
            .TransitionTo(OrderState.PaymentApproved)
    
    // PaymentApproved: Gateway that decides based on payment result
    .For(OrderState.PaymentApproved)
        .Immediately()
            .Guard(data => data.PaymentApproved)
            .TransitionTo(OrderState.ProcessingShipment)
        .Immediately()
            .Guard(data => !data.PaymentApproved)
            .Execute(data => new OrderCommand.CancelOrder(data.OrderId, "Payment declined"))
            .TransitionTo(OrderState.PaymentDeclined)
        .Done()
    
    // PaymentDeclined: Terminal state
    .For(OrderState.PaymentDeclined)
        // Order cancelled
    
    // ProcessingShipment: Ship the order
    .For(OrderState.ProcessingShipment)
        .OnEntry(data => new OrderCommand.ShipOrder(data.OrderId))
        .On<OrderTrigger.Shipped>()
            .Execute(data => new OrderCommand.SendConfirmation(data.OrderId))
            .TransitionTo(OrderState.Completed)
    
    // Completed: Terminal state
    .For(OrderState.Completed)
        // Order complete
    
    .Build();

// Usage scenario

var orderData = new OrderData(
    OrderId: Guid.NewGuid(),
    InventoryAvailable: false,  // Not set yet
    PaymentApproved: false,     // Not set yet
    Total: 99.99m);

// Step 1: Start the order (immediate transition to ValidatingInventory)
var (state1, data1, cmds1) = machine.Start(orderData);
// state1 == OrderState.ValidatingInventory
// Immediate transition from Submitted → ValidatingInventory happened
// cmds1 == [CheckInventory(orderId)]

// Step 2: Inventory check returns (available)
var (state2, data2, cmds2) = machine.Fire(
    new OrderTrigger.InventoryChecked(Available: true),
    state1,
    data1);
// state2 == OrderState.InventoryOk
// data2.InventoryAvailable == true

// Step 3: Immediate transition evaluates (need to call Start again)
var (state3, data3, cmds3) = machine.Start(data2);
// state3 == OrderState.ValidatingPayment
// Immediate transition from InventoryOk → ValidatingPayment happened
// cmds3 == [ProcessPayment(orderId, 99.99)]

// Step 4: Payment processed (approved)
var (state4, data4, cmds4) = machine.Fire(
    new OrderTrigger.PaymentProcessed(Approved: true),
    state3,
    data3);
// state4 == OrderState.PaymentApproved
// data4.PaymentApproved == true

// Step 5: Immediate transition evaluates
var (state5, data5, cmds5) = machine.Start(data4);
// state5 == OrderState.ProcessingShipment
// Immediate transition from PaymentApproved → ProcessingShipment happened
// cmds5 == [ShipOrder(orderId)]

// Step 6: Order shipped
var (state6, data6, cmds6) = machine.Fire(
    new OrderTrigger.Shipped(),
    state5,
    data5);
// state6 == OrderState.Completed
// cmds6 == [SendConfirmation(orderId)]

// Alternate scenario: Inventory not available
var (stateAlt, dataAlt, cmdsAlt) = machine.Fire(
    new OrderTrigger.InventoryChecked(Available: false),
    state1,
    data1);
// stateAlt == OrderState.InventoryOk
var (stateFinal, dataFinal, cmdsFinal) = machine.Start(dataAlt);
// stateFinal == OrderState.InsufficientInventory
// Immediate transition took the "not available" path
// cmdsFinal includes CancelOrder(orderId, "Out of stock")
```

**What's happening:**
1. Gateway states (`Submitted`, `InventoryOk`, `PaymentApproved`) use immediate transitions
2. No manual triggers needed for these decision points
3. Guards on immediate transitions route based on data
4. Validation states wait for external triggers with results
5. Clean separation between waiting states and decision states

---

## Best Practices

✅ **Use for gateway/decision states**  
States whose only purpose is to route based on conditions.

✅ **Call Start() to trigger immediate transitions**  
After Fire() if you need immediate transitions to evaluate.

✅ **Guard immediate transitions**  
Always use guards unless unconditional transition is intended.

✅ **Keep immediate transition logic simple**  
Complex logic belongs in guards, not in chained immediate transitions.

❌ **Avoid infinite loops**  
Don't create cycles of immediate transitions.

❌ **Don't overuse**  
Not every state needs an immediate transition. Use triggers for external events.

---

## Common Patterns

### Initialization state
```csharp
.For(State.Starting)
    .OnEntry(() => new Command.Initialize())
    .Immediately()
        .TransitionTo(State.Ready)
        .Done()
```

### Gateway with multiple paths
```csharp
.For(State.Gateway)
    .Immediately()
        .Guard(data => data.Condition1)
        .TransitionTo(State.Path1)
    .Immediately()
        .Guard(data => data.Condition2)
        .TransitionTo(State.Path2)
    .Immediately()  // No guard = catch-all
        .TransitionTo(State.DefaultPath)
    .Done()
```

### Conditional setup
```csharp
.For(State.Setup)
    .OnEntry(() => new Command.LoadResources())
    .Immediately()
        .Guard(data => data.ResourcesLoaded)
        .TransitionTo(State.Active)
        .Done()
```

---

## Next Steps

- Combine with [Guards](Guards-and-Conditional-Flows.md) to understand condition evaluation
- See [Hierarchical States](Hierarchical-States.md) where substates can have immediate transitions
- Learn about [Entry/Exit Commands](Entry-and-Exit-Commands.md) that run during immediate transitions
