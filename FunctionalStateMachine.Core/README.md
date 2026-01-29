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
- Support for sub-state machines
- Fast, efficient rehydration

## Example Usage

```csharp
var builder = new StateMachineBuilder<MyState, MyTrigger, MyData, CommandBase>()
    .StartWith(MyState.Initial);

var initial = builder.For(MyState.Initial);
initial.OnEntry(state => Command.DoSomething(state.Data.Id))
    .OnExit(state => Command.DoSomethingElse(state.Data.Id));
initial.On(MyTrigger.Trigger1)
    .TransitionTo(MyState.State1)
    .Execute((state, trigger) => Command.DoSomething(state.Data.Id));
initial.On(MyTrigger.Trigger2)
    .Guard((state, trigger) => !state.Data.SeenIds.Contains(trigger.Id))
    .WithData((state, trigger) => state.Data with { SeenIds = state.Data.SeenIds.Add(trigger.Id) })
    .TransitionTo(MyState.State2)
    .Execute((state, trigger) => Command.DoSomething(state.Data.Id));
initial.On(MyTrigger.Trigger3)
    .TransitionTo(MyState.Initial);

var stateMachine = builder.Build();
var current = new State<MyState, MyData>(MyState.Initial, new MyData("abc-123"));
var (newState, commands) = stateMachine.Fire(MyTrigger.Trigger1, current);
```
