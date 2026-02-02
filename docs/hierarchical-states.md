# Hierarchical States

Model parent/child relationships so parent transitions apply while in any child, and the parent chooses an initial child.

## Why it is useful

- Captures real-world state hierarchies.
- Reduces duplicate transitions across child states.
- Enables reusable parent behavior.

## Simple example

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Active)
        .StartsWith(State.Anonymous)
        .On(Trigger.Timeout)
            .TransitionTo(State.Expired)
    .For(State.Anonymous)
        .SubStateOf(State.Active)
        .On(Trigger.Login)
            .TransitionTo(State.Authenticated)
    .For(State.Authenticated)
        .SubStateOf(State.Active)
        .On(Trigger.Logout)
            .TransitionTo(State.Anonymous)
    .Build();
```
