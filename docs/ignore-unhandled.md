# Ignore and Unhandled Triggers

You can explicitly ignore triggers or handle them with a global callback.

## Why it is useful

- Keeps behavior explicit for unsupported triggers.
- Lets you centralize logging or metrics for unhandled cases.
- Avoids unexpected exceptions when you choose to ignore.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .OnUnhandled()
        .Ignore()
    .For(State.Ready)
        .On(Trigger.Ping)
            .Ignore()
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .OnUnhandled()
        .Execute((trigger, state) =>
            new Command.LogUnhandled(trigger, state))
    .For(State.Active)
        .On(Trigger.Stop)
            .TransitionTo(State.Stopped)
    .Build();
```
