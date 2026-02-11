# State Data and ModifyData

State machines can carry data alongside the current state, allowing you to track information that evolves through transitions. The `ModifyData` method updates this data immutably during transitions.

## Table of Contents

1. [Why Use State Data](#why-use-state-data)
2. [Basic Data Tracking](#basic-data-tracking)
3. [Modifying Data During Transitions](#modifying-data-during-transitions)
4. [Accessing Data in Commands](#accessing-data-in-commands)
5. [Multiple Data Modifications](#multiple-data-modifications)
6. [ModifyData Signatures](#modifydata-signatures)
7. [Complete Example](#complete-example)

---

## Why Use State Data

**Track context** — Remember information between state changes (counters, IDs, timestamps, domain data)  
**Atomic updates** — Data and state change together in one operation  
**Immutable by design** — Data is never mutated, only replaced with new versions  
**Pure transitions** — All data changes are visible and deterministic

---

## Basic Data Tracking

Start by defining a data record and attaching it to your state machine:

```csharp
public enum CounterState { Idle, Counting }

public abstract record CounterTrigger
{
    public sealed record Start : CounterTrigger;
    public sealed record Increment : CounterTrigger;
    public sealed record Stop : CounterTrigger;
}

// Data record - must be immutable
public sealed record CounterData(int Count);

public abstract record CounterCommand
{
    public sealed record Display(int Count) : CounterCommand;
}

var machine = StateMachine<CounterState, CounterTrigger, CounterData, CounterCommand>
    .Create()
    .StartWith(CounterState.Idle)
    .For(CounterState.Idle)
        .On<CounterTrigger.Start>()
            .TransitionTo(CounterState.Counting)
    .Build();

// Start with initial data
var data = new CounterData(Count: 0);
var (newState, newData, commands) = machine.Fire(
    new CounterTrigger.Start(), 
    CounterState.Idle, 
    data);
// newState == CounterState.Counting
// newData.Count == 0 (unchanged)
```

**Key points:**
- Data is passed separately from state: `Fire(trigger, state, data)`
- Data should be immutable (use `record` types)
- Data flows through every transition

---

## Modifying Data During Transitions

Use `.ModifyData()` to update data as part of a transition:

```csharp
var machine = StateMachine<CounterState, CounterTrigger, CounterData, CounterCommand>
    .Create()
    .StartWith(CounterState.Idle)
    .For(CounterState.Counting)
        .On<CounterTrigger.Increment>()
            .ModifyData(data => data with { Count = data.Count + 1 })
    .Build();

var data = new CounterData(Count: 5);
var (newState, newData, commands) = machine.Fire(
    new CounterTrigger.Increment(), 
    CounterState.Counting, 
    data);
// newState == CounterState.Counting (no TransitionTo = internal transition)
// newData.Count == 6
```

**How it works:**
- `.ModifyData()` takes the current data and returns new data
- Uses C# record `with` syntax for immutable updates
- The new data is returned from `Fire()` along with the new state

**Why immutable?** Immutability ensures every state change is deterministic and replayable. No hidden mutations.

---

## Accessing Data in Commands

Commands can use the updated data:

```csharp
var machine = StateMachine<CounterState, CounterTrigger, CounterData, CounterCommand>
    .Create()
    .StartWith(CounterState.Counting)
    .For(CounterState.Counting)
        .On<CounterTrigger.Increment>()
            .ModifyData(data => data with { Count = data.Count + 1 })
            .Execute(data => new CounterCommand.Display(data.Count))  // Uses NEW data
    .Build();

var data = new CounterData(Count: 5);
var (newState, newData, commands) = machine.Fire(
    new CounterTrigger.Increment(), 
    CounterState.Counting, 
    data);
// newData.Count == 6
// commands == [Display(6)]  ← Command sees updated data
```

**Important:** `Execute` steps run *after* `ModifyData`, so they see the updated data.

---

## Using Trigger Data

Triggers can carry information that influences data updates:

```csharp
public abstract record CounterTrigger
{
    public sealed record IncrementBy(int Amount) : CounterTrigger;
}

var machine = StateMachine<CounterState, CounterTrigger, CounterData, CounterCommand>
    .Create()
    .StartWith(CounterState.Counting)
    .For(CounterState.Counting)
        .On<CounterTrigger.IncrementBy>()
            .ModifyData((data, trigger) => data with 
            { 
                Count = data.Count + trigger.Amount 
            })
            .Execute(data => new CounterCommand.Display(data.Count))
    .Build();

var data = new CounterData(Count: 10);
var (newState, newData, commands) = machine.Fire(
    new CounterTrigger.IncrementBy(Amount: 5), 
    CounterState.Counting, 
    data);
// newData.Count == 15
// commands == [Display(15)]
```

---

## Multiple Data Modifications

Combine multiple data updates in a single transition:

```csharp
public sealed record SessionData(
    Guid UserId, 
    DateTime LastActivity, 
    int RequestCount);

public abstract record SessionTrigger
{
    public sealed record MakeRequest : SessionTrigger;
}

var machine = StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand>
    .Create()
    .For(SessionState.Active)
        .On<SessionTrigger.MakeRequest>()
            .ModifyData(data => data with 
            {
                LastActivity = DateTime.UtcNow,
                RequestCount = data.RequestCount + 1
            })
            .Execute(data => new SessionCommand.LogActivity(data.RequestCount))
    .Build();
```

**Pattern:** Use record `with` expressions to update multiple properties at once while keeping the object immutable.

---

## ModifyData Signatures

Like guards and execute steps, `ModifyData` supports multiple overloads:

### 1. Data only

```csharp
.ModifyData(data => data with { Count = data.Count + 1 })
```

Use when the update only depends on current data.

### 2. State and data

```csharp
.ModifyData((state, data) => data with 
{ 
    Count = state == CounterState.Counting ? data.Count + 1 : data.Count 
})
```

Use when the update depends on the current state (rare).

### 3. Data and trigger

```csharp
.ModifyData((data, trigger) => data with 
{ 
    Count = data.Count + trigger.Amount 
})
```

Use when the trigger carries information needed for the update.

### 4. State, data, and trigger

```csharp
.ModifyData((state, data, trigger) => data with 
{
    Count = state == CounterState.Counting 
        ? data.Count + trigger.Amount 
        : trigger.Amount
})
```

Use when you need all three pieces of information (rare).

---

## Complete Example

A shopping cart state machine that tracks items and totals:

```csharp
public enum CartState
{
    Empty,
    Active,
    CheckingOut,
    Completed
}

public abstract record CartTrigger
{
    public sealed record AddItem(string ItemId, decimal Price) : CartTrigger;
    public sealed record RemoveItem(string ItemId) : CartTrigger;
    public sealed record ApplyDiscount(decimal Percentage) : CartTrigger;
    public sealed record Checkout : CartTrigger;
    public sealed record ConfirmPayment : CartTrigger;
}

public sealed record CartData(
    Dictionary<string, decimal> Items,
    decimal Subtotal,
    decimal DiscountPercentage,
    DateTime LastModified)
{
    // Helper property
    public decimal Total => Subtotal * (1 - DiscountPercentage);
}

public abstract record CartCommand
{
    public sealed record UpdateDisplay(decimal Total, int ItemCount) : CartCommand;
    public sealed record ProcessPayment(decimal Amount) : CartCommand;
    public sealed record SendReceipt(decimal Total) : CartCommand;
}

var machine = StateMachine<CartState, CartTrigger, CartData, CartCommand>.Create()
    .StartWith(CartState.Empty)
    
    .For(CartState.Empty)
        .On<CartTrigger.AddItem>()
            .ModifyData((data, trigger) =>
            {
                var newItems = new Dictionary<string, decimal>(data.Items)
                {
                    [trigger.ItemId] = trigger.Price
                };
                return data with
                {
                    Items = newItems,
                    Subtotal = trigger.Price,
                    LastModified = DateTime.UtcNow
                };
            })
            .Execute(data => new CartCommand.UpdateDisplay(data.Total, data.Items.Count))
            .TransitionTo(CartState.Active)
    
    .For(CartState.Active)
        .On<CartTrigger.AddItem>()
            .ModifyData((data, trigger) =>
            {
                var newItems = new Dictionary<string, decimal>(data.Items)
                {
                    [trigger.ItemId] = trigger.Price
                };
                return data with
                {
                    Items = newItems,
                    Subtotal = data.Subtotal + trigger.Price,
                    LastModified = DateTime.UtcNow
                };
            })
            .Execute(data => new CartCommand.UpdateDisplay(data.Total, data.Items.Count))
        
        .On<CartTrigger.RemoveItem>()
            .ModifyData((data, trigger) =>
            {
                var newItems = new Dictionary<string, decimal>(data.Items);
                var price = newItems[trigger.ItemId];
                newItems.Remove(trigger.ItemId);
                
                return data with
                {
                    Items = newItems,
                    Subtotal = data.Subtotal - price,
                    LastModified = DateTime.UtcNow
                };
            })
            .Execute(data => new CartCommand.UpdateDisplay(data.Total, data.Items.Count))
        
        .On<CartTrigger.ApplyDiscount>()
            .ModifyData((data, trigger) => data with
            {
                DiscountPercentage = trigger.Percentage / 100m,
                LastModified = DateTime.UtcNow
            })
            .Execute(data => new CartCommand.UpdateDisplay(data.Total, data.Items.Count))
        
        .On<CartTrigger.Checkout>()
            .TransitionTo(CartState.CheckingOut)
    
    .For(CartState.CheckingOut)
        .On<CartTrigger.ConfirmPayment>()
            .Execute(data => new CartCommand.ProcessPayment(data.Total))
            .Execute(data => new CartCommand.SendReceipt(data.Total))
            .TransitionTo(CartState.Completed)
    
    .Build();

// Usage scenario
var emptyCart = new CartData(
    Items: new Dictionary<string, decimal>(),
    Subtotal: 0,
    DiscountPercentage: 0,
    LastModified: DateTime.UtcNow);

// Add first item (transitions to Active)
var (state1, cart1, cmds1) = machine.Fire(
    new CartTrigger.AddItem("WIDGET-1", 29.99m),
    CartState.Empty,
    emptyCart);
// state1 == CartState.Active
// cart1.Subtotal == 29.99m
// cart1.Items.Count == 1

// Add second item
var (state2, cart2, cmds2) = machine.Fire(
    new CartTrigger.AddItem("WIDGET-2", 19.99m),
    state1,
    cart1);
// state2 == CartState.Active
// cart2.Subtotal == 49.98m
// cart2.Items.Count == 2

// Apply 10% discount
var (state3, cart3, cmds3) = machine.Fire(
    new CartTrigger.ApplyDiscount(10),
    state2,
    cart2);
// state3 == CartState.Active
// cart3.DiscountPercentage == 0.1m
// cart3.Total == 44.982m (49.98 * 0.9)

// Checkout
var (state4, cart4, cmds4) = machine.Fire(
    new CartTrigger.Checkout(),
    state3,
    cart3);
// state4 == CartState.CheckingOut

// Confirm payment
var (state5, cart5, cmds5) = machine.Fire(
    new CartTrigger.ConfirmPayment(),
    state4,
    cart4);
// state5 == CartState.Completed
// cmds5 == [ProcessPayment(44.982), SendReceipt(44.982)]
```

**What's happening:**
1. Data tracks items, subtotal, discount, and timestamp
2. Each trigger modifies relevant data properties immutably
3. Commands use updated data to display totals
4. All data evolution is explicit and traceable
5. No hidden state—everything flows through data

---

## Best Practices

✅ **Use immutable records for data**  
C# record types with `with` expressions make immutable updates easy.

✅ **Keep data focused**  
Only store information that affects state machine decisions or commands.

✅ **Update data atomically with transitions**  
Data changes happen as part of transitions, keeping everything synchronized.

✅ **Use trigger data for external input**  
Triggers carry information from the outside world into data.

❌ **Don't perform I/O in ModifyData**  
ModifyData should be a pure function—no database calls, no API requests.

❌ **Don't mutate data in place**  
Always return new data instances. Never modify existing data objects.

---

## Common Patterns

### Counter/Accumulator
```csharp
.ModifyData(data => data with { Count = data.Count + 1 })
```

### Timestamp Tracking
```csharp
.ModifyData(data => data with { LastUpdated = DateTime.UtcNow })
```

### Collection Management
```csharp
.ModifyData((data, trigger) => data with 
{ 
    Items = data.Items.Append(trigger.NewItem).ToList() 
})
```

### Conditional Updates
```csharp
.ModifyData((data, trigger) => data with
{
    Score = trigger.Success ? data.Score + 10 : data.Score
})
```

---

## Next Steps

- Use data in [Guards](guards.md) to make decisions based on data values
- Combine with [Conditional Steps](conditional-steps.md) for complex data transformations
- See [Execute Steps](execute-steps.md) to emit commands using updated data
