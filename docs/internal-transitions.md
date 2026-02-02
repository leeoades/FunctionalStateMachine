# Internal Transitions

If you omit `TransitionTo`, the state does not change. Entry/exit actions will not run, but data and commands still can.

## Why it is useful

- Lets you model in-place updates.
- Keeps entry/exit commands for true state changes only.
- Supports high-frequency events like ticks.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Running)
        .On(Trigger.Tick)
            .ModifyData(data => data with { Count = data.Count + 1 })
            .Execute(data => new Command.Log($"Tick {data.Count + 1}"))
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Connected)
        .On(Trigger.Ping)
            .Execute((state, data, trigger) => new Command.Metric("ping"))
            .ModifyData(data => data with { LastSeen = DateTimeOffset.UtcNow })
    .Build();
```
