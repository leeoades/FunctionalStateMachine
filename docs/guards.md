# Guards and Conditional Flows

Guards let you choose between multiple transitions for the same trigger, based on state or data.

## Why it is useful

- Encodes business rules directly in the transition.
- Supports branching without external if/else logic.
- Allows guarded transitions in both data and no-data machines.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Pending)
        .On(Trigger.Submit)
            .Guard(data => data.Score > 70)
            .TransitionTo(State.Approved)
        .On(Trigger.Submit)
            .Guard(data => data.Score <= 70)
            .TransitionTo(State.Manual)
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Pending)
        .On(Trigger.Submit)
            .Guard((state, data, trigger) => data.IsVip && trigger.Force)
            .TransitionTo(State.Approved)
        .On(Trigger.Submit)
            .Guard((state, data, trigger) => !data.IsVip)
            .TransitionTo(State.Review)
    .Build();
```
