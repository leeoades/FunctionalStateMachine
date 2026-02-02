# Entry and Exit Commands

Entry and exit actions generate commands when a state changes. This is useful for emitting notifications or side effects outside the machine.

## Why it is useful

- Centralizes lifecycle actions per state.
- Keeps transitions pure but expressive.
- Supports both data-only and state-aware actions.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Ready)
        .OnEntry(() => new Command.Log("Enter Ready"))
        .OnExit(() => new Command.Log("Exit Ready"))
    .Build();
```

## More complex example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Active)
        .OnEntry((state, data) => new Command.Audit($"Enter {state} for {data.UserId}"))
        .OnExit((state, data) => new Command.Audit($"Exit {state} with {data.UserId}"))
    .Build();
```
