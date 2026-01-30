# Functional State Machine
## A different type of state machine

This library provides a functional style of state machine.
The declaration of the state machine is still done using typical fluent syntax.
The difference is that given a current state, plus a trigger, the state machine will return the next state plus any commands to execute.
The commands are logical representations of side effects and are passed to a CommandHandler for execution.

The origins for the requirement for this style of state machine stem from its use
within an actor model based system within a finance system. Instead of having a long-lived
memory resident state machine, the actor loads the persisted state from storage,
rehydrates the state machine, processes the trigger, persists the state and executes the commands.

Traditional state machines include the calls to external services to perform the side effects.
This means that unit testing the state machine requires mocking those external services.
Instead, because this library returns a logical representation of the side effects,
the unit tests are more straightforward and can focus on testing the intent.

They also include a method for extracting the state for persistence almost as an afterthought.

Another requirement we regularly encountered was the need to store additional information alongside the state.
For example, if a trigger included an `id` then we needed to be able to retrieve the `id` at a later state change.
This state machine allows for storing additional information alongside the state.

Features:
- Fluent syntax
- No external dependencies
- Enter and Exit actions
- Hierarchical states
- Fast, efficient rehydration

## Example Usage

```csharp
public abstract record MyTrigger
{
    public sealed record Trigger1 : MyTrigger;
    public sealed record Trigger2(Guid Id) : MyTrigger;
    public sealed record Trigger3 : MyTrigger;

    public static readonly MyTrigger Trigger1 = new Trigger1();
    public static readonly MyTrigger Trigger3 = new Trigger3();

    public static MyTrigger Trigger2(Guid id) => new Trigger2(id);
}

var stateMachine = StateMachine<MyState, MyTrigger, MyData, CommandBase>.Create()
    .StartWith(MyState.Initial)
    .For(MyState.Initial)
        .OnEntry(state => Command.DoSomething(state.Data.Id))
        .OnExit(state => Command.DoSomethingElse(state.Data.Id))
        .On(MyTrigger.Trigger1)
            .TransitionTo(MyState.State1)
            .Execute(state => Command.DoSomething(state.Data.Id))
        .On<MyTrigger.Trigger2>()
            .Guard((state, trigger) => !state.Data.SeenIds.Contains(trigger.Id))
            .ModifyData((state, trigger) => state.Data with { SeenIds = state.Data.SeenIds.Add(trigger.Id) })
            .TransitionTo(MyState.State2)
            .Execute(state => Command.DoSomething(state.Data.Id))
        .On(MyTrigger.Trigger3)
            .TransitionTo(MyState.Initial)
    .Build();

var current = new State<MyState, MyData>(MyState.Initial, new MyData("abc-123"));
var (newState, commands) = stateMachine.Fire(MyTrigger.Trigger1, current);
```
