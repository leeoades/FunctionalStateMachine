# Execute Steps and Multiple Commands

You can emit commands from Execute steps, choosing from several overloads and returning one or many commands.

## Why it is useful

- Keeps command creation close to the transition logic.
- Supports composable actions and batching.
- Works with or without access to state, data, or trigger.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .On(Trigger.Start)
            .Execute(() => new Command.Log("Started"))
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .On(Trigger.Start)
            .Execute((state, data, trigger) => new Command.Log($"{state} {data.Id}"))
            .Execute((data, trigger) => new Command.Notify(data.Owner))
            .Execute(data => new Command[]
            {
                new Command.Audit(data.Id),
                new Command.Metric("started")
            })
    .Build();
```
