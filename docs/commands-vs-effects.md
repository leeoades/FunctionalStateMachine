# Commands Instead of Side Effects

The state machine returns commands that describe what should happen next. Your application decides how and when to execute them.

Note: Also included in this library is a Command Dispatcher. See [Command Runners](docs/command-runners.md).

## Why it is useful

- Keeps transitions pure and deterministic.
- Makes persistence and replay straightforward.
- Keeps side effects in your application layer.

## Simple example

```csharp
public abstract record HomeCommand
{
    public sealed record TurnOnLights() : HomeCommand;
}

var machine = StateMachine<State, Trigger, Data, HomeCommand>.Create()
    .For(State.Empty)
        .On(Trigger.PersonEnter)
            .Execute(data => new HomeCommand.TurnOnLights())
            .TransitionTo(State.Active)
    .Build();
```

## More complex example

```csharp
public abstract record BillingCommand
{
    public sealed record Charge(decimal Amount) : BillingCommand;
    public sealed record SendReceipt(string Email) : BillingCommand;
}

var machine = StateMachine<State, Trigger, Data, BillingCommand>.Create()
    .For(State.Pending)
        .On(Trigger.Submit)
            .Execute(data => new BillingCommand.Charge(data.Total))
            .Execute(data => new BillingCommand.SendReceipt(data.Email))
            .TransitionTo(State.Complete)
    .Build();
```
