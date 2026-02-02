# State Data and ModifyData

Attach data to your state so transitions can update it deterministically alongside state changes.

## Why it is useful

- Keeps state and data evolution in one place.
- Produces predictable, replayable outcomes.
- Supports both state-aware and data-only updates.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Running)
        .On(Trigger.Tick)
            .ModifyData(data => data with { Count = data.Count + 1 })
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Running)
        .On(Trigger.AddItem)
            .ModifyData((state, data, trigger) =>
                data with { Items = data.Items.Append(trigger.Item).ToList() })
            .Execute(data => new Command.ItemsUpdated(data.Items))
    .Build();
```
