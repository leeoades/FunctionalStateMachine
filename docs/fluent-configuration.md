# Fluent Configuration

The builder API expresses state/trigger behavior in a readable chain and locks configuration once you call `Build()`.

## Why it is useful

- Keeps configuration centralized and explicit.
- Ensures validation happens before runtime.
- Reads like a specification of behavior.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .StartWith(State.Idle)
    .For(State.Idle)
        .On(Trigger.Start)
            .TransitionTo(State.Running)
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Pending)
    .For(State.Pending)
        .On(Trigger.Submit)
            .Guard(data => data.IsValid)
            .TransitionTo(State.Approved)
        .On(Trigger.Submit)
            .Guard(data => !data.IsValid)
            .TransitionTo(State.Rejected)
    .For(State.Approved)
        .On(Trigger.Cancel)
            .TransitionTo(State.Canceled)
    .Build();
```
