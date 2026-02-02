# No-Data Builder

If you do not need extra data, the no-data builder avoids carrying a data type while keeping the same fluent API.

## Why it is useful

- Keeps models small and readable.
- Avoids boilerplate data records.
- Works with all core features.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .StartWith(State.Off)
    .For(State.Off)
        .On(Trigger.Toggle)
            .TransitionTo(State.On)
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Ready)
        .On(Trigger.Start)
            .Execute(state => new Command.Log($"Start from {state}"))
            .TransitionTo(State.Running)
    .For(State.Running)
        .On(Trigger.Stop)
            .TransitionTo(State.Ready)
    .Build();
```
