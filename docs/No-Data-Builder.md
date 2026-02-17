# No-Data Builder

When your state machine doesn't need to track data, use the no-data builder to keep your configuration simpler. You get all the same features—guards, commands, hierarchies—without the data type parameter.

## Table of Contents

1. [Why Use No-Data Builder](#why-use-no-data-builder)
2. [Basic No-Data Machine](#basic-no-data-machine)
3. [Features Available](#features-available)
4. [When to Use Data vs No-Data](#when-to-use-data-vs-no-data)
5. [Converting Between Data and No-Data](#converting-between-data-and-no-data)
6. [Complete Example](#complete-example)

---

## Why Use No-Data Builder

**Simpler types** — Three type parameters instead of four  
**Less boilerplate** — No need to define and pass data records  
**Clearer intent** — Signals that state alone drives behavior  
**Same features** — Guards, hierarchies, entry/exit all still work

---

## Basic No-Data Machine

Compare the two approaches:

### With Data (4 type parameters)

```csharp
public sealed record LightData(int UsageCount);

var machine = StateMachine<LightState, LightTrigger, LightData, LightCommand>
    .Create()
    .StartWith(LightState.Off)
    .For(LightState.Off)
        .On<LightTrigger.Toggle>()
            .TransitionTo(LightState.On)
    .Build();

// Must pass data when firing
var data = new LightData(UsageCount: 0);
var (newState, newData, commands) = machine.Fire(
    new LightTrigger.Toggle(),
    LightState.Off,
    data);
```

### Without Data (3 type parameters)

```csharp
var machine = StateMachine<LightState, LightTrigger, LightCommand>
    .Create()
    .StartWith(LightState.Off)
    .For(LightState.Off)
        .On<LightTrigger.Toggle>()
            .TransitionTo(LightState.On)
    .Build();

// No data to pass
var (newState, commands) = machine.Fire(
    new LightTrigger.Toggle(),
    LightState.Off);
// Simpler signature! ✅
```

**Key difference:** `Fire()` doesn't take or return data.

---

## Features Available

All core features work with no-data machines:

### ✅ Transitions

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Start)
        .On<Trigger.Next>()
            .TransitionTo(State.End)
    .Build();
```

### ✅ Execute Steps

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Active)
        .On<Trigger.Action>()
            .Execute(() => new Command.DoWork())
            .Execute(() => new Command.Log("Work done"))
    .Build();
```

### ✅ Entry and Exit Commands

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Processing)
        .OnEntry(() => new Command.StartTimer())
        .OnExit(() => new Command.StopTimer())
    .Build();
```

### ✅ Guards (state-based)

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Gateway)
        .On<Trigger.Route>()
            .Guard(state => state == State.Gateway)  // Can check state
            .TransitionTo(State.PathA)
    .Build();
```

### ✅ Hierarchical States

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Parent)
        .StartsWith(State.Child1)
        .On<Trigger.Exit>()
            .TransitionTo(State.Outside)
    .For(State.Child1)
        .SubStateOf(State.Parent)
    .Build();
```

### ✅ Internal Transitions

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Running)
        .On<Trigger.Tick>()
            .Execute(() => new Command.RecordTick())
            // No TransitionTo = internal transition
    .Build();
```

### ✅ Immediate Transitions

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Starting)
        .Immediately()
            .TransitionTo(State.Ready)
            .Done()
    .Build();
```

### ❌ ModifyData

Not available—there's no data to modify!

---

## When to Use Data vs No-Data

### Use **No-Data** when:

✅ State alone captures all information  
✅ No need to track counters, IDs, timestamps, etc.  
✅ Simpler configuration is valuable  
✅ Commands carry all necessary context

**Examples:**
- Simple on/off switches
- Basic workflow states (draft → review → approved)
- Connection states (connected/disconnected)
- UI navigation (menu → settings → game)

### Use **Data** when:

✅ Need to track evolving information  
✅ Guards depend on data values  
✅ Commands need data context  
✅ State + data together define the "state"

**Examples:**
- Shopping cart (need items, totals)
- User session (need user ID, timestamps)
- Download progress (need byte counts)
- Game state (need score, lives, level)

---

## Guard Signatures in No-Data Machines

Guards can still use state and trigger:

```csharp
public abstract record RouteTrigger
{
    public sealed record Go(string Destination) : RouteTrigger;
}

var machine = StateMachine<State, RouteTrigger, Command>.Create()
    .For(State.Gateway)
        .On<RouteTrigger.Go>()
            .Guard(trigger => trigger.Destination == "home")
            .TransitionTo(State.Home)
        .On<RouteTrigger.Go>()
            .Guard(trigger => trigger.Destination == "settings")
            .TransitionTo(State.Settings)
    .Build();
```

**Available guard signatures:**
- `Guard(state => ...)`
- `Guard(trigger => ...)`
- `Guard((state, trigger) => ...)`

**Not available:** Any signature with `data =>` parameter.

---

## Execute Signatures in No-Data Machines

Execute steps can use state and trigger:

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Active)
        .On<Trigger.Action>()
            .Execute(() => new Command.Log("Simple"))                    // No params
            .Execute(state => new Command.Log($"State: {state}"))        // State only
            .Execute(trigger => new Command.Log($"Trigger: {trigger}"))  // Trigger only
            .Execute((state, trigger) => 
                new Command.Log($"{state} -> {trigger}"))                // Both
    .Build();
```

**Not available:** Any signature with `data =>` parameter.

---

## Converting Between Data and No-Data

### Adding Data Later

If you start with no-data and need to add data:

```csharp
// Before (no-data)
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Active)
        .On<Trigger.Action>()
            .Execute(() => new Command.DoWork())
    .Build();

// After (with data)
public sealed record StateData(int Count);

var machine = StateMachine<State, Trigger, StateData, Command>.Create()
    .For(State.Active)
        .On<Trigger.Action>()
            .ModifyData(data => data with { Count = data.Count + 1 })
            .Execute(data => new Command.DoWork(data.Count))
    .Build();
```

**Migration cost:** Moderate. Need to:
1. Define data record
2. Update type parameters
3. Update `Fire()` calls to pass data
4. Optionally use data in guards/execute

### Removing Data

If you have data but don't need it:

```csharp
// Before (with data)
public sealed record StateData(int Count);

var machine = StateMachine<State, Trigger, StateData, Command>.Create()
    .For(State.Active)
        .On<Trigger.Action>()
            .Execute(() => new Command.DoWork())  // Not using data!
    .Build();

// After (no-data)
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Active)
        .On<Trigger.Action>()
            .Execute(() => new Command.DoWork())
    .Build();
```

**When safe:** If no transitions use data (no `ModifyData`, guards don't check data, execute doesn't use data).

---

## Complete Example

A traffic light state machine without data:

```csharp
public enum TrafficLightState
{
    Red,
    Yellow,
    Green
}

public abstract record TrafficLightTrigger
{
    public sealed record TimerExpired : TrafficLightTrigger;
    public sealed record Emergency : TrafficLightTrigger;
    public sealed record EmergencyClear : TrafficLightTrigger;
}

public abstract record TrafficLightCommand
{
    public sealed record SetLight(TrafficLightState Color) : TrafficLightCommand;
    public sealed record StartTimer(int Seconds) : TrafficLightCommand;
    public sealed record FlashRed : TrafficLightCommand;
    public sealed record StopFlashing : TrafficLightCommand;
    public sealed record Log(string Message) : TrafficLightCommand;
}

var machine = StateMachine<TrafficLightState, TrafficLightTrigger, TrafficLightCommand>
    .Create()
    .StartWith(TrafficLightState.Red)
    
    .For(TrafficLightState.Red)
        .OnEntry(() => new TrafficLightCommand.SetLight(TrafficLightState.Red))
        .OnEntry(() => new TrafficLightCommand.StartTimer(30))
        .OnExit(() => new TrafficLightCommand.Log("Leaving Red"))
        .On<TrafficLightTrigger.TimerExpired>()
            .Execute(() => new TrafficLightCommand.Log("Red -> Green"))
            .TransitionTo(TrafficLightState.Green)
        .On<TrafficLightTrigger.Emergency>()
            .Execute(() => new TrafficLightCommand.FlashRed())
            .TransitionTo(TrafficLightState.Red)  // Self-transition triggers entry/exit
    
    .For(TrafficLightState.Green)
        .OnEntry(() => new TrafficLightCommand.SetLight(TrafficLightState.Green))
        .OnEntry(() => new TrafficLightCommand.StartTimer(45))
        .OnExit(() => new TrafficLightCommand.Log("Leaving Green"))
        .On<TrafficLightTrigger.TimerExpired>()
            .Execute(() => new TrafficLightCommand.Log("Green -> Yellow"))
            .TransitionTo(TrafficLightState.Yellow)
        .On<TrafficLightTrigger.Emergency>()
            .Execute(() => new TrafficLightCommand.FlashRed())
            .TransitionTo(TrafficLightState.Red)
    
    .For(TrafficLightState.Yellow)
        .OnEntry(() => new TrafficLightCommand.SetLight(TrafficLightState.Yellow))
        .OnEntry(() => new TrafficLightCommand.StartTimer(5))
        .OnExit(() => new TrafficLightCommand.Log("Leaving Yellow"))
        .On<TrafficLightTrigger.TimerExpired>()
            .Execute(() => new TrafficLightCommand.Log("Yellow -> Red"))
            .TransitionTo(TrafficLightState.Red)
        .On<TrafficLightTrigger.Emergency>()
            .Execute(() => new TrafficLightCommand.FlashRed())
            .TransitionTo(TrafficLightState.Red)
    
    .Build();

// Usage

// Normal operation
var (state1, cmds1) = machine.Fire(
    new TrafficLightTrigger.TimerExpired(),
    TrafficLightState.Red);
// state1 == TrafficLightState.Green
// cmds1 == [
//   Log("Leaving Red"),
//   SetLight(Green),
//   StartTimer(45),
//   Log("Red -> Green")
// ]

// Emergency override
var (state2, cmds2) = machine.Fire(
    new TrafficLightTrigger.Emergency(),
    state1);
// state2 == TrafficLightState.Red
// cmds2 == [
//   Log("Leaving Green"),
//   Log("Leaving Red"),      // Exit old Red (self-transition)
//   SetLight(Red),           // Entry new Red
//   StartTimer(30),
//   FlashRed()
// ]

// Timer continues after emergency
var (state3, cmds3) = machine.Fire(
    new TrafficLightTrigger.TimerExpired(),
    state2);
// state3 == TrafficLightState.Green
```

**What's happening:**
1. No data needed—state alone defines behavior
2. Entry commands set the light color and timer
3. All transitions handled by timer or emergency
4. Self-transition on emergency triggers entry/exit
5. Simple, clean configuration without data clutter

---

## Best Practices

✅ **Start with no-data**  
Default to no-data unless you know you need data.

✅ **Use commands to carry context**  
Instead of storing in data, pass information through commands.

✅ **Use triggers to carry information**  
Trigger properties can carry one-time information.

✅ **Consider external state**  
Sometimes data lives outside the state machine (database, cache, etc.).

❌ **Don't force no-data**  
If you need to track evolving information, use data.

❌ **Don't duplicate data in commands**  
If you're passing the same information in every command, it's probably data.

---

## Common Patterns

### Simple workflow
```csharp
StateMachine<WorkflowState, WorkflowTrigger, WorkflowCommand>.Create()
    .For(State.Draft)
        .On<Trigger.Submit>()
            .TransitionTo(State.Review)
```

### Connection state
```csharp
StateMachine<ConnectionState, ConnectionTrigger, ConnectionCommand>.Create()
    .For(State.Disconnected)
        .On<Trigger.Connect>()
            .Execute(() => new Command.OpenSocket())
            .TransitionTo(State.Connected)
```

### UI navigation
```csharp
StateMachine<Screen, NavigationTrigger, NavigationCommand>.Create()
    .For(Screen.MainMenu)
        .On<Trigger.SelectSettings>()
            .Execute(() => new Command.ShowSettings())
            .TransitionTo(Screen.Settings)
```

---

## Next Steps

- Compare with [State Data](State-Data-and-ModifyData.md) to understand when data is useful
- See [Guards](Guards-and-Conditional-Flows.md) for state and trigger-based guards
- Learn about [Execute Steps](Execute-Steps-and-Multiple-Commands.md) for emitting commands without data
