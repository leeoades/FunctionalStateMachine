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

## Features at a glance

- Fluent configuration and validation
- Entry/exit commands
- Guards, conditional branches, and ignores
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

- Commands instead of side effects: [docs/commands-vs-effects.md](docs/commands-vs-effects.md)
- Fluent configuration: [docs/fluent-configuration.md](docs/fluent-configuration.md)
- Entry and exit commands: [docs/entry-exit.md](docs/entry-exit.md)
- Guards and conditional flows: [docs/guards.md](docs/guards.md)
- State data and ModifyData: [docs/state-data.md](docs/state-data.md)
- Execute steps and multiple commands: [docs/execute-steps.md](docs/execute-steps.md)
- Ignore and unhandled triggers: [docs/ignore-unhandled.md](docs/ignore-unhandled.md)
- Internal transitions: [docs/internal-transitions.md](docs/internal-transitions.md)
- Hierarchical states: [docs/hierarchical-states.md](docs/hierarchical-states.md)
- No-data builder: [docs/no-data.md](docs/no-data.md)
- Mermaid diagram generation: [docs/diagrams.md](docs/diagrams.md)
- Command runners: [docs/command-runners.md](docs/command-runners.md)

## Additional

- See Samples in `FunctionalStateMachine.Samples` project.

