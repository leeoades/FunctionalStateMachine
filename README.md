# Functional State Machine

Welcome! This library provides a functional, persistence-friendly state machine for .NET.
Instead of executing side effects directly, transitions return commands that your app can handle later.
That keeps the state machine pure, easy to test, and great for rehydrated or actor-style systems.

## Quick Start

Build the machine with a fluent builder, then call `Build()` to freeze the configuration.

```csharp
var builder = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .StartWith(OrderState.Created);

builder.For(OrderState.Created)
    .OnEntry(state => new AuditCommand($"Entering {state.Value}"))
    .On(OrderTrigger.Pay)
        .TransitionTo(OrderState.Paid)
        .Execute(state => new ChargeCommand(state.Data.OrderId));

builder.For(OrderState.Paid)
    .On(OrderTrigger.Ship)
        .TransitionTo(OrderState.Shipped)
        .Execute(state => new ShipCommand(state.Data.OrderId));

var machine = builder.Build();

var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-100"));
var (next, commands) = machine.Fire(OrderTrigger.Pay, current);
```

## Why this State Machine?

Compared to traditional state machines (including popular libraries like Stateless), this library:

- Returns commands instead of performing side effects.
- Keeps transitions pure and deterministic.
- Makes unit testing straightforward (no mocked services).
- Supports rehydration (persist state + data, rebuild machine at runtime).
- Allows extra data to travel with the state.

## Features and Examples

### 1) Pure commands instead of side effects

Commands are logical descriptions of work to do, not the work itself.

```csharp
public abstract record OrderCommand;
public sealed record ChargeCommand(string OrderId) : OrderCommand;
public sealed record ShipCommand(string OrderId) : OrderCommand;
```

The state machine returns these commands for your handler to execute.

### 2) Fluent configuration with a build step

Configuration is done up front and `Build()` seals the machine.

```csharp
var builder = StateMachine<State, Trigger, Data, Command>.Create();
builder.For(State.Ready).On(Trigger.Start).TransitionTo(State.Running);
var machine = builder.Build();
```

After `Build()`, no more configuration is possible.

### 3) Entry and exit commands

Entry and exit actions yield commands when state changes.

```csharp
builder.For(State.Ready)
    .OnEntry(() => new LogCommand("Enter Ready"))
    .OnExit(() => new LogCommand("Exit Ready"));
```

### 4) Guards and multiple transitions

You can define multiple transitions for a trigger and gate them with guards.

```csharp
builder.For(State.Pending)
    .On(Trigger.Submit)
        .Guard((state, trigger) => state.Data.Score > 70)
        .TransitionTo(State.Manual)
    .On(Trigger.Submit)
        .Guard((state, trigger) => state.Data.Score <= 70)
        .TransitionTo(State.Approved);
```

### 5) Update state data during transitions

Attach data to your state and update it as transitions happen.

```csharp
builder.For(State.Pending)
    .On(Trigger.Submit)
    .WithData(state => state.Data with { Notes = "High risk" })
        .TransitionTo(State.Manual);
```

### 6) Multiple Execute overloads

Choose the most convenient `Execute` shape for each case.

```csharp
builder.For(State.Ready).On(Trigger.Start)
    .Execute(() => new LogCommand("No args"))
    .Execute((Trigger trigger) => new LogCommand($"Trigger: {trigger}"))
    .Execute((State<State, Data> state) => new LogCommand($"State: {state.Value}"))
    .Execute((state, trigger) => new LogCommand("Both"));
```

### 7) Multiple commands from a single action

Return one or many commands from a transition.

```csharp
builder.For(State.Ready).On(Trigger.Start)
    .Execute(() => new Command[]
    {
        new LogCommand("One"),
        new LogCommand("Two")
    });
```

### 8) Ignore triggers

Ignore a trigger cleanly without a state change.

```csharp
builder.For(State.Ready)
    .On(Trigger.Ping)
        .Ignore();
```

### 9) Unhandled trigger policy

Provide a handler or let it throw.

```csharp
var builder = StateMachine<State, Trigger, Data, Command>.Create()
    .OnUnhandled((trigger, state) => state.Data.Log.Add($"Unhandled:{trigger}"));
```

### 10) Internal transitions

If you omit `TransitionTo`, you stay in the same state (entry/exit do not run).

```csharp
builder.For(State.Running).On(Trigger.Tick)
    .WithData(state => state.Data with { Count = state.Data.Count + 1 })
    .Execute(state => new LogCommand($"Tick {state.Data.Count + 1}"));
```

### 11) Sub-state machines

Compose a state machine inside another with shared triggers.

```csharp
builder.For(ParentState.Active)
    .WithSubStateMachine(
        childMachine,
        data => data.Child,
        (data, sub) => data with { Child = sub })
    .On(Trigger.Timeout)
        .TransitionTo(ParentState.Expired);
```

### 12) No extra data case

If you do not need extra data, use the NoData builder.

```csharp
var builder = StateMachine<State, Trigger, Command>.Create()
    .StartWith(State.Off);
```

## Where to look next

- Samples: `FunctionalStateMachine.Samples/README.md`
- Core API: `FunctionalStateMachine.Core/StateMachineBuilder.cs`

## How this differs from Stateless (at a glance)

- Functional result: returns commands instead of executing side effects.
- Data-carrying states: keep extra context alongside state.
- Built for rehydration: configuration is static, state is portable.
- Testable by design: pure transitions, no mocks needed for side effects.
