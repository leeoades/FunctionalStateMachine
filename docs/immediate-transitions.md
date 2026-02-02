# Immediate transitions

Immediate transitions let a state advance without a trigger as soon as it is entered. This is useful for setup states,
gateway states, or flows where you want to emit a command and move on automatically.

## Why use it?

- Avoid placeholder triggers for "start" or "bootstrap" states.
- Keep setup logic in the state machine rather than in calling code.
- Model guard-based branching without waiting for an external event.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Starting)
    .For(State.Starting)
        .OnExit(data => new Command.Log("Starting up"))
        .Immediately()
            .TransitionTo(State.WaitingForInput)
            .Done()
    .For(State.WaitingForInput)
    .Build();

var (state, data, commands) = machine.Start(Data.Initial);
```

## Guarded immediate transition

```csharp
.For(State.Checking)
    .Immediately()
        .Guard(data => data.HasAccess)
        .TransitionTo(State.Allowed)
        .Done()
```

## Notes

- Immediate transitions run only when a state is entered.
- Use `Start(...)` (or `Start()` for no-data machines) to run entry actions and immediate transitions from the initial state.
- If no immediate transition guard matches, the machine stays in the current state.
