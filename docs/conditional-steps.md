# Conditional Steps (If / ElseIf / Else)

Conditional steps let you branch within a single trigger's action, executing different commands, data modifications, or state transitions based on a predicate.

## Why it is useful

- Handle multiple scenarios within a single transition without creating separate transitions.
- Execute conditional commands or data updates based on state and trigger data.
- More natural and readable than multiple guarded transitions.
- Cleaner code when you have many related conditions.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.PaymentReview)
        .On<Trigger.PayTrigger>()
            .If((data, trigger) => trigger.Amount >= 10m)
                .Execute(() => new Command.AcceptPayment())
                .Done()
    .Build();
```

When the condition is true, the commands in the If block execute. When false, nothing happens and the transition continues.

## If with Else

Handle both the true and false cases:

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.PaymentReview)
        .On<Trigger.PayTrigger>()
            .If((data, trigger) => trigger.Amount >= 10m)
                .Execute(() => new Command.AcceptPayment())
                .Else()
                .Execute((data, trigger) => new Command.RequestMorePayment(10m - trigger.Amount))
                .Done()
    .Build();
```

## If with ElseIf and Else

Handle multiple conditions in sequence. The first matching condition's steps execute:

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.PaymentReview)
        .On<Trigger.PayTrigger>()
            .If((data, trigger) => trigger.Amount >= 20m)
                .Execute(() => new Command.VIPPayment())
                .ElseIf((data, trigger) => trigger.Amount >= 10m)
                .Execute(() => new Command.StandardPayment())
                .ElseIf((data, trigger) => trigger.Amount >= 5m)
                .Execute(() => new Command.BasicPayment())
                .Else()
                .Execute(() => new Command.MinimumPayment())
                .Done()
    .Build();
```

## Conditional TransitionTo

You can transition to a new state inside an If/ElseIf/Else chain. Each execution path may only include one TransitionTo.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Pending)
    .For(State.Pending)
        .On<Trigger.Submit>()
            .If(data => data.IsValid)
                .TransitionTo(State.Approved)
                .Else()
                .Execute(() => new Command.LogRejected())
                .Done()
    .Build();
```

With multiple ElseIf conditions, only the first matching branch executes. If no condition matches and there's no Else, nothing happens.

## Multiple conditional chains with TransitionTo

If you use multiple conditional chains within the same transition, only one chain may contain a TransitionTo.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Pending)
    .For(State.Pending)
        .On<Trigger.Submit>()
            .If(data => data.IsValid)
                .TransitionTo(State.Approved)
                .Else()
                .Execute(() => new Command.LogRejected())
                .Done()
            .If(data => data.IsHighPriority)
                .TransitionTo(State.Escalated) // ❌ second TransitionTo in same transition
                .Else()
                .Execute(() => new Command.LogPriority())
                .Done()
    .Build();
```

## Using with ModifyData

Conditional data modifications work seamlessly:

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Inventory)
        .On<Trigger.StockCheck>()
            .If((data, trigger) => data.Quantity > 100)
                .ModifyData(data => data with { Status = "Overstocked" })
                .Execute(() => new Command.RequestReorder())
                .ElseIf((data, trigger) => data.Quantity < 10)
                .ModifyData(data => data with { Status = "LowStock" })
                .Execute(() => new Command.Reorder())
                .Else()
                .ModifyData(data => data with { Status = "Normal" })
                .Done()
    .Build();
```

## Predicate signatures

All predicates support the same overloads as the main fluent API:

```csharp
// These all work:
.If((data, trigger) => ...)          // Data and trigger
.If((state, data, trigger) => ...)   // State, data, and trigger
.If(data => ...)                      // Just data
.If((state, data) => ...)             // State and data
```

Same for `ElseIf`:

```csharp
.ElseIf((data, trigger) => ...)
.ElseIf((state, data, trigger) => ...)
.ElseIf(data => ...)
.ElseIf((state, data) => ...)
```

## Multiple Execute steps

You can chain multiple Execute or ModifyData calls in each branch:

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Checkout)
        .On<Trigger.Complete>()
            .If((data, trigger) => data.Total > 100m)
                .ModifyData(data => data with { ApplyDiscount = true })
                .Execute(data => new Command.ApplyDiscount(data.Total * 0.1m))
                .Execute(() => new Command.GrantFreeShip())
                .ElseIf((data, trigger) => data.IsVIP)
                .Execute(() => new Command.GrantFreeShip())
                .Else()
                .Execute(() => new Command.RequestShipping())
                .Done()
    .Build();
```
