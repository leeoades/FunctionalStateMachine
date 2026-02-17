# Conditional Steps (If / ElseIf / Else)

Conditional steps let you branch within a single trigger's action, executing different commands, data modifications, or state transitions based on a predicate. This provides fine-grained control within a transition without creating multiple guarded transitions.

## Table of Contents

1. [Why Use Conditional Steps](#why-use-conditional-steps)
2. [Basic If Statement](#basic-if-statement)
3. [If with Else](#if-with-else)
4. [If with ElseIf and Else](#if-with-elseif-and-else)
5. [Conditional TransitionTo](#conditional-transitionto)
6. [Using with ModifyData](#using-with-modifydata)
7. [Predicate Signatures](#predicate-signatures)
8. [Multiple Execute Steps](#multiple-execute-steps)
9. [Best Practices](#best-practices)

---

## Why Use Conditional Steps

## Why Use Conditional Steps

**Handle multiple scenarios** — Branch within a single transition without creating separate transitions  
**Execute conditional commands** — Run different commands based on state and trigger data  
**More natural and readable** — Cleaner than multiple guarded transitions for related conditions  
**Fine-grained control** — Combine with ModifyData and Execute for complex branching

---

## Basic If Statement

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

---

## Best Practices

✅ **Use for variations within a transition**  
When branches go to the same state or stay in current state, If/Else is cleaner than guards.

✅ **Chain multiple Execute and ModifyData**  
Each branch can have multiple steps.

✅ **Use guards for different target states**  
If conditions lead to completely different states, use guards instead.

✅ **Keep conditions simple**  
Complex logic should be extracted to methods.

❌ **Don't nest If statements**  
Use ElseIf instead for flat, readable conditions.

❌ **Don't mix too many concerns**  
If your If/ElseIf chain is very long, consider refactoring.

---

## Next Steps

- Compare with [Guards](Guards-and-Conditional-Flows.md) to understand when to use each approach
- Combine with [ModifyData](State-Data-and-ModifyData.md) for conditional data updates
- Use with [Execute Steps](Execute-Steps-and-Multiple-Commands.md) for conditional command emission
