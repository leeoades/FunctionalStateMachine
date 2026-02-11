# Functional State Machine

A functional-style, persistence-friendly state machine for .NET. 
Transitions produce logical commands instead of performing side effects, keeping your workflow deterministic, 
testable, and easy to replay.

## Why use this library?

- Pure transitions: return commands instead of doing work.
- Simple persistence: The state is not locked inside the machine.
- Testable by design: no mocked dependencies needed to validate behaviour.
- Hierarchical states: model parent/child flows supported.
- Optional design-time tooling: generate Mermaid diagrams from your builder code.

## Main Use Case

The driving scenario for this library was for a way to declare and design behaviour
for components that operate within an actor model. 

Actor instances are brought into existence, rehydrated from persisted state, triggered
by messages, perform actions, and then persist their state again. Whilst the actor instance
may be reused, this is not guaranteed, hence why state machines that expect to stay memory
resident are suboptimal.

The advantage of using this state machine to model the behaviour is that it provides a clear
and consistent approach between components, and can take advantage of analysis tools such 
as detecting issues with the flow logic and the ability to generate flow diagrams at compile time. 

## Features at a glance

- Fluent configuration and validation
- Entry/exit commands
- Guards, conditional branches, and ignores
- Immediate transitions
- Data attached to state, updated in transitions
- Hierarchical states and internal transitions
- Mermaid diagram generator
- Command runners for dispatching commands via DI

## Getting started

Reference the core package and build a machine:

```csharp
public enum LightState
{
    Off,
    On
}

public abstract record LightTrigger
{
    public sealed record Toggle : LightTrigger;
}

public sealed record LightData(int NumberOfUsages);

public abstract record LightCommand
{
    public sealed record SwitchOn : LightCommand;
    public sealed record SwitchOff : LightCommand;
}

var machine = StateMachine<LightState, LightTrigger, LightData, LightCommand>
    .Create()
        .StartWith(LightState.Off)
        .For(LightState.Off)
            .On<LightTrigger.Toggle>()
                .TransitionTo(LightState.On)
                .ModifyData(data => data with { NumberOfUsages = data.NumberOfUsages + 1 })
                .Execute(() => new LightCommand.SwitchOn())
        .For(LightState.On)
            .On<LightTrigger.Toggle>()
                .TransitionTo(LightState.Off)
                .Execute(() => new LightCommand.SwitchOff())
        .Build();

var currentState = LightState.Off;
var currentData = new LightData(NumberOfUsages: 0);

var (nextState, nextData, commands) = machine.Fire(new LightTrigger.Toggle(), currentState, currentData);
```

## Documentation

Each feature has its own short guide with simple and more advanced examples.

- Packages and when to reference them: [docs/packages.md](docs/packages.md)
- Commands instead of side effects: [docs/commands-vs-effects.md](docs/commands-vs-effects.md)
- Fluent configuration: [docs/fluent-configuration.md](docs/fluent-configuration.md)
- Entry and exit commands: [docs/entry-exit.md](docs/entry-exit.md)
- Guards and conditional flows: [docs/guards.md](docs/guards.md)
- State data and ModifyData: [docs/state-data.md](docs/state-data.md)
- Execute steps and multiple commands: [docs/execute-steps.md](docs/execute-steps.md)
- Ignore and unhandled triggers: [docs/ignore-unhandled.md](docs/ignore-unhandled.md)
- Internal transitions: [docs/internal-transitions.md](docs/internal-transitions.md)
- Immediate transitions: [docs/immediate-transitions.md](docs/immediate-transitions.md)
- Hierarchical states: [docs/hierarchical-states.md](docs/hierarchical-states.md)
- No-data builder: [docs/no-data.md](docs/no-data.md)
- Mermaid diagram generation: [docs/diagrams.md](docs/diagrams.md)
- Command runners: [docs/command-runners.md](docs/command-runners.md)

## Additional

- See Samples in `src/FunctionalStateMachine.Samples`, plus `src/VendingMachineSampleApp` and `src/StockPurchaserSampleApp`.
