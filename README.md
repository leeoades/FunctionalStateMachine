# Functional State Machine

Welcome! This library provides a functional, persistence-friendly state machine for .NET.
Instead of executing side effects directly, transitions return commands that your app can handle separately.
That keeps the state machine pure, easy to test, and great for actor-style systems.

## Quick Start

Build the machine with a fluent builder.

```csharp
public enum LightState
{
    Off,
    On
}

public enum LightTrigger
{
    Toggle
}

var stateMachine = 
    StateMachine<LightState, LightTrigger, LightCommand>
        .Create()
            .StartWith(LightState.Off)
            .For(LightState.Off)
                .On(LightTrigger.Toggle)
                    .TransitionTo(LightState.On)
                    .Execute(() => new LightCommand.SwitchOn())
            .For(LightState.On)
                .On(LightTrigger.Toggle)
                    .TransitionTo(LightState.Off)
                    .Execute(() => new LightCommand.SwitchOff())
            .Build();

```

## Why this State Machine?

Compared to traditional state machines (including popular libraries like Stateless), this library:

- Returns logical commands instead of performing side effects.
- Keeps transitions pure and deterministic.
- Makes unit testing straightforward (no mocked services).
- No "Rehydration" - current state is passed in, not locked in.  
- Allows extra data to travel with the state.

## Features and Examples

### 1) Pure commands instead of side effects

Commands are logical descriptions of work to do, not the work itself.

```csharp
public abstract record ShopCommand
{
    public sealed record CartUpdated(IReadOnlyList<LineItem> Items) : ShopCommand;
    public sealed record TotalCalculated(decimal Total) : ShopCommand;
    public sealed record PaymentRequested : ShopCommand;
    public sealed record PaymentFailed : ShopCommand;
    public sealed record OwnershipGranted(IReadOnlyList<LineItem> Items) : ShopCommand;
}
```

The state machine returns these commands for your handler to execute.

### 2) Fluent configuration

Configuration is done up front and `Build()` seals the machine.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .On(Trigger.Start)
            .TransitionTo(State.Running)
    .Build();
```

After `Build()`, no more configuration is possible.

### 3) Entry and exit commands

Entry and exit actions yield commands when state changes.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .OnEntry(() => new LogCommand("Enter Ready"))
        .OnExit(() => new LogCommand("Exit Ready"))
    .Build();
```

### 4) Guards and multiple transitions

You can define multiple transitions for a trigger and gate them with guards.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Pending)
        .On(Trigger.Submit)
            .Guard((state, trigger) => state.Data.Score > 70)
            .TransitionTo(State.Manual)
        .On(Trigger.Submit)
            .Guard((state, trigger) => state.Data.Score <= 70)
            .TransitionTo(State.Approved)
    .Build();
```

### 5) Update state data during transitions

Attach data to your state and update it as transitions happen.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Pending)
        .On(Trigger.Submit)
            .WithData(state => state.Data with { Notes = "High risk" })
            .TransitionTo(State.Manual)
    .Build();
```

### 6) Multiple Execute overloads

Choose the most convenient `Execute` shape for each case.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .On(Trigger.Start)
            .Execute(() => new LogCommand("No args"))
            .Execute((Trigger trigger) => new LogCommand($"Trigger: {trigger}"))
            .Execute((State<State, Data> state) => new LogCommand($"State: {state.Value}"))
            .Execute((state, trigger) => new LogCommand("Both"))
    .Build();
```

### 7) Multiple commands from a single action

Return one or many commands from a transition.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .On(Trigger.Start)
            .Execute(() => new Command[]
            {
                new LogCommand("One"),
                new LogCommand("Two")
            })
    .Build();
```

### 8) Ignore triggers

Ignore a trigger cleanly without a state change.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .On(Trigger.Ping)
            .Ignore()
    .Build();
```

### 9) Unhandled trigger policy

Provide a handler or let it throw.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .OnUnhandled((trigger, state) => state.Data.Log.Add($"Unhandled:{trigger}"))
    .Build();
```

### 10) Internal transitions

If you omit `TransitionTo`, you stay in the same state (entry/exit do not run).

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Running)
        .On(Trigger.Tick)
            .WithData(state => state.Data with { Count = state.Data.Count + 1 })
            .Execute(state => new LogCommand($"Tick {state.Data.Count + 1}"))
    .Build();
```

### 11) Sub-state machines

Compose a state machine inside another with shared triggers.

```csharp
var machine = StateMachine<ParentState, Trigger, ParentData, Command>.Create()
    .For(ParentState.Active)
        .WithSubStateMachine(
            childMachine,
            data => data.Child,
            (data, sub) => data with { Child = sub })
        .On(Trigger.Timeout)
            .TransitionTo(ParentState.Expired)
    .Build();
```

### 12) No extra data case

If you do not need extra data, use the NoData builder.

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .StartWith(State.Off)
    .Build();
```

## Where to look next

- Samples: `FunctionalStateMachine.Samples/README.md`
- Core API: `FunctionalStateMachine.Core/StateMachineBuilder.cs`

## How this differs from Stateless (at a glance)

- Functional result: returns commands instead of executing side effects.
- Data-carrying states: keep extra context alongside state.
- Built for rehydration: configuration is static, state is portable.
- Testable by design: pure transitions, no mocks needed for side effects.
