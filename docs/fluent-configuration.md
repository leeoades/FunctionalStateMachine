# Fluent Configuration

The fluent builder API lets you express state machine behavior in a readable, chainable syntax. Configuration is validated when you call `.Build()`, catching errors before runtime.

## Table of Contents

1. [Why Fluent Configuration](#why-fluent-configuration)
2. [Basic State Machine](#basic-state-machine)
3. [Adding Multiple States](#adding-multiple-states)
4. [Adding Triggers and Transitions](#adding-triggers-and-transitions)
5. [Validation on Build](#validation-on-build)
6. [Complete Example](#complete-example)

---

## Why Fluent Configuration

**Centralized** — All behavior defined in one place, not scattered across classes  
**Validated** — Catches configuration errors (unreachable states, missing transitions) before runtime  
**Readable** — Reads like a specification: "For state X, on trigger Y, transition to Z"  
**Chainable** — Natural method chaining reduces boilerplate

---

## Basic State Machine

Start with the simplest possible machine: one state, one trigger.

```csharp
public enum TrafficLightState { Red, Green }

public abstract record TrafficLightTrigger
{
    public sealed record Change : TrafficLightTrigger;
}

public abstract record TrafficLightCommand
{
    public sealed record ShowRed : TrafficLightCommand;
    public sealed record ShowGreen : TrafficLightCommand;
}

var machine = StateMachine<TrafficLightState, TrafficLightTrigger, TrafficLightCommand>
    .Create()                              // 1. Create a builder
        .StartWith(TrafficLightState.Red)  // 2. Set initial state
        .For(TrafficLightState.Red)        // 3. Configure state
            .On<TrafficLightTrigger.Change>()     // 4. Handle trigger
                .TransitionTo(TrafficLightState.Green)  // 5. Specify next state
        .Build();                          // 6. Build and validate
```

**Step by step:**
1. `Create()` — Returns a builder for your state machine
2. `StartWith()` — Defines which state the machine starts in
3. `For()` — Begins configuring a specific state
4. `On<>()` — Specifies which trigger this state should handle
5. `TransitionTo()` — Defines the target state for this transition
6. `Build()` — Validates configuration and creates the state machine

---

## Adding Multiple States

Expand to multiple states by chaining `.For()` calls:

```csharp
// Yes, I know this isn't how a traffic light sequence goes...
var machine = StateMachine<TrafficLightState, TrafficLightTrigger, TrafficLightCommand>
    .Create()
        .StartWith(TrafficLightState.Red)
        .For(TrafficLightState.Red)
            .On<TrafficLightTrigger.Change>()
                .TransitionTo(TrafficLightState.Amber)
        .For(TrafficLightState.Amber)                    // Add another state
            .On<TrafficLightTrigger.Change>()
                .TransitionTo(TrafficLightState.Green)
        .For(TrafficLightState.Green)                     // And another
            .On<TrafficLightTrigger.Change>()
                .TransitionTo(TrafficLightState.Red)      // Cycles back
        .Build();
```

Each `.For()` block defines one state and its transitions. Order doesn't matter—define states in any sequence.

---

## Adding Triggers and Transitions

States can handle multiple triggers:

```csharp
public abstract record TrafficLightTrigger
{
    public sealed record TimerExpired : TrafficLightTrigger;
    public sealed record EmergencyStop : TrafficLightTrigger;
    public sealed record Resume : TrafficLightTrigger;
}

var machine = StateMachine<TrafficLightState, TrafficLightTrigger, TrafficLightCommand>
    .Create()
        .StartWith(TrafficLightState.Red)
        .For(TrafficLightState.Red)
            .On<TrafficLightTrigger.TimerExpired>()
                .TransitionTo(TrafficLightState.Green)
            .On<TrafficLightTrigger.EmergencyStop>()      // Multiple triggers per state
                .TransitionTo(TrafficLightState.Red)      // Can transition to self
        .For(TrafficLightState.Green)
            .On<TrafficLightTrigger.TimerExpired>()
                .TransitionTo(TrafficLightState.Amber)
            .On<TrafficLightTrigger.EmergencyStop>()
                .TransitionTo(TrafficLightState.Red)
        .Build();
```

**Key points:**
- Each `.On<>()` defines one trigger handler
- Multiple handlers can exist for the same state
- A state can transition to itself (self-transition)
- Not all states need to handle all triggers

---

## Validation on Build

The `.Build()` method validates your configuration:

```csharp
// ❌ This will throw at Build()
var machine = StateMachine<State, Trigger, Command>.Create()
    .StartWith(State.Start)
    .For(State.Start)
        .On<Trigger.Next>()
            .TransitionTo(State.Middle)
    .For(State.End)  // ❌ ERROR: 'End' is unreachable!
        .On<Trigger.Reset>()
            .TransitionTo(State.Start)
    .Build();  // Throws: "State 'End' is unreachable from initial state 'Start'"
```

**Validation catches:**
- Unreachable states (no inbound transitions)
- States configured but never used
- Orphaned states (not part of any path)
- Duplicate transition definitions

**Fix:** Add a transition to make the state reachable:

```csharp
.For(State.Middle)
    .On<Trigger.Finish>()
        .TransitionTo(State.End)  // ✅ Now 'End' is reachable
```

---

## Complete Example

A complete door lock with multiple states, triggers, and validation:

```csharp
public enum DoorState
{
    Locked,
    Unlocked,
    Jammed
}

public abstract record DoorTrigger
{
    public sealed record InsertKey : DoorTrigger;
    public sealed record Turn : DoorTrigger;
    public sealed record RemoveKey : DoorTrigger;
    public sealed record ForceLock : DoorTrigger;
    public sealed record Repair : DoorTrigger;
}

public abstract record DoorCommand
{
    public sealed record UnlockBolt : DoorCommand;
    public sealed record LockBolt : DoorCommand;
    public sealed record Beep : DoorCommand;
    public sealed record AlarmSound : DoorCommand;
}

var machine = StateMachine<DoorState, DoorTrigger, DoorCommand>.Create()
    .StartWith(DoorState.Locked)
    
    .For(DoorState.Locked)
        .On<DoorTrigger.InsertKey>()
            .Execute(() => new DoorCommand.UnlockBolt())
            .Execute(() => new DoorCommand.Beep())
            .TransitionTo(DoorState.Unlocked)
        .On<DoorTrigger.ForceLock>()                      // Force causes jam
            .Execute(() => new DoorCommand.AlarmSound())
            .TransitionTo(DoorState.Jammed)
    
    .For(DoorState.Unlocked)
        .On<DoorTrigger.RemoveKey>()
            .Execute(() => new DoorCommand.LockBolt())
            .TransitionTo(DoorState.Locked)
        .On<DoorTrigger.Turn>()                           // Turning while unlocked jams it
            .Execute(() => new DoorCommand.AlarmSound())
            .TransitionTo(DoorState.Jammed)
    
    .For(DoorState.Jammed)
        .On<DoorTrigger.Repair>()
            .Execute(() => new DoorCommand.LockBolt())
            .TransitionTo(DoorState.Locked)
    
    .Build();  // ✅ All states reachable, no errors

// Use the machine
var (newState, commands) = machine.Fire(new DoorTrigger.InsertKey(), DoorState.Locked);
// newState == DoorState.Unlocked
// commands == [UnlockBolt, Beep]
```

**What's happening:**
- Three states, each handling relevant triggers
- Commands emitted during transitions (`.Execute()`)
- Multiple commands can be chained
- All states are reachable from the initial state
- Validation passes ✅

---

## Next Steps

- Add business logic with [Guards](guards.md)
- Attach data to states with [State Data](state-data.md)
- Run actions on state entry/exit with [Entry/Exit Commands](entry-exit.md)
