# Copilot Instructions for Functional State Machine

## Project Overview

A functional-style, persistence-friendly state machine library for .NET. Transitions return logical commands instead of performing side effects, enabling deterministic, testable, and replayable workflows. Designed primarily for actor-model architectures where state machines don't remain resident in memory.

## Build & Test Commands

### Build the solution
```bash
dotnet build
```

### Run all tests
```bash
dotnet test
```

### Run tests for a specific project
```bash
# Core functionality tests
dotnet test FunctionalStateMachine.Core.Tests

# Command runner tests
dotnet test FunctionalStateMachine.CommandRunnerTests

# Diagram generation tests
dotnet test FunctionalStateMachine.Diagrams.Tests
```

### Run a single test file
```bash
dotnet test FunctionalStateMachine.Core.Tests --filter FullyQualifiedName~StateMachineFireTests
```

### Run with code coverage
```bash
dotnet test /p:CollectCoverage=true
```

## Architecture

### Core Design Pattern

The state machine uses a **fluent builder pattern** with four generic type parameters:

- **TState** - enum or class representing states
- **TTrigger** - abstract record hierarchy for trigger types
- **TData** - record type holding state-associated data
- **TCommand** - abstract record hierarchy for output commands

Example structure:
```csharp
public enum MyState { StateA, StateB }
public abstract record MyTrigger { public sealed record Activate : MyTrigger; }
public sealed record MyData(int Counter);
public abstract record MyCommand { public sealed record DoWork : MyCommand; }

var machine = StateMachine<MyState, MyTrigger, MyData, MyCommand>.Create()
    .StartWith(MyState.StateA)
    .For(MyState.StateA)
        .On<MyTrigger.Activate>()
            .TransitionTo(MyState.StateB)
            .Execute(() => new MyCommand.DoWork())
    .Build();
```

### Fire Execution Model

The `.Fire()` method is pure and stateless:
- **Input**: trigger, currentState, currentData
- **Output**: tuple of (newState, newData, commands)
- **Effect**: Returns a list of commands to be executed elsewhere; performs no side effects itself

### Key Architectural Layers

1. **FunctionalStateMachine.Core** - Core builder and state machine logic
2. **FunctionalStateMachine.Diagrams** - Mermaid diagram generation from machine definitions
3. **FunctionalStateMachine.CommandRunner** - DI integration for command dispatch
4. **FunctionalStateMachine.CommandRunner.Generator** - Source generator for command dispatchers
5. **VendingMachineSampleApp** - Interactive sample demonstrating command runners
6. **FunctionalStateMachine.Samples** - Example implementations (LightSwitch, Timer, ShoppingTrolley, SessionLogin)

## Key Conventions

### Test Organization

Tests are organized by feature in `FunctionalStateMachine.Core.Tests`:
- `StateMachineFireTests` - basic transition and data modification
- `StateMachineConditionalTests` - guard conditions and branches (`If`/`Else`/`Done`)
- `StateMachineHierarchyTests` - parent/child state relationships
- `StateMachineImmediateTransitionTests` - automatic transitions
- `StateMachineValidationTests` - fluent builder validation
- `StateMachineUnhandledTests` - unhandled trigger behavior

Use xUnit with minimal setup—tests are pure function tests requiring no mocks or DI.

### Fluent Builder Chaining

The builder uses a fluent pattern where `For(state)` returns a `StateConfiguration` that supports:
- `.On<TTrigger>()` - defines trigger handler for specific trigger type
- `.OnEntry()` - entry commands when entering state (multiple overloads for different signatures)
- `.OnExit()` - exit commands when leaving state
- `.TransitionTo()` - target state for transition
- `.ModifyData()` - functional update to state data
- `.Execute()` / `.ExecuteSteps()` - return command(s)
- `.If()` / `.ElseIf()` / `.Else()` / `.Done()` - conditional branches

### Record-Based Types

The library heavily uses C# records:
- **States**: typically enums, but can be records
- **Triggers, Data, Commands**: always records
- Trigger and Command types use sealed record inheritance for polymorphic dispatch
- `.ModifyData()` uses `with` expressions for immutable updates

### Data Binding in Transitions

Data updates flow through the builder:
```csharp
.On<MyTrigger.Increment>()
    .ModifyData(data => data with { Count = data.Count + 1 })
    .Execute(/* receives updated data context if needed */)
```

### Validation

The `.Build()` method validates:
- Initial state is set
- All referenced states are defined
- Trigger handlers are properly configured

Validation errors throw `InvalidOperationException`.

### Hierarchical States

Parent/child relationships are defined via builder:
```csharp
.For(ParentState)
    .WithInitialSubState(ChildState)
    // configuration...
```

Child states can have their own transitions; unhandled triggers propagate to parent.

### Immediate Transitions

Use `.ImmediateTransitionTo()` for automatic state transitions (useful for entry actions that conditionally route to different states).

### Command Dispatching

Commands returned from `.Fire()` are dispatched separately:
- `FunctionalStateMachine.CommandRunner` provides `ICommandDispatcher<TCommand>`
- `FunctionalStateMachine.CommandRunner.Generator` generates dispatchers via source generation
- Register handlers via `services.AddCommandRunners<TCommand>()` with a generator reference configured as an analyzer.

## Documentation

Feature-specific guides are in `/docs`:
- `fluent-configuration.md` - builder API overview
- `commands-vs-effects.md` - why commands instead of side effects
- `guards.md` - conditional transitions
- `conditional-steps.md` - if/elseif/else branching within transitions
- `hierarchical-states.md` - parent/child state structures
- `diagrams.md` - generating Mermaid visualizations
- `command-runners.md` - DI integration patterns

## MCP Server Configuration

For enhanced C# development support, configure OmniSharp or Roslyn-based language servers:

### OmniSharp (Recommended for C# LSP)
```bash
# Install OmniSharp
dotnet tool install -g OmniSharp

# In .github/lsp.json:
{
  "lspServers": {
    "csharp": {
      "command": "omnisharp",
      "args": ["-lsp"],
      "fileExtensions": {
        ".cs": "csharp"
      }
    }
  }
}
```

This provides:
- Go-to-definition for state/trigger/command/data types
- Hover information for fluent API
- Diagnostics for builder validation issues
- Rename refactoring across state machine definitions

## Common Tasks

### Add a new transition
1. Add trigger type to trigger record hierarchy
2. Use `.On<NewTrigger>()` in appropriate state configuration
3. Chain `.TransitionTo()`, `.ModifyData()`, `.Execute()` as needed
4. Add test in relevant test file

### Add hierarchical state
1. Define parent and child state enum values
2. Use `.For(ParentState).WithInitialSubState(ChildState)`
3. Configure child transitions under parent or separately with `.For(ChildState)`
4. Test with `StateMachineHierarchyTests` pattern

### Generate Mermaid diagram
Use `FunctionalStateMachine.Diagrams.MermaidGenerator` (see samples for examples).

### Add command handler
Define a concrete record class extending the command hierarchy and add `ICommandRunner<TCommand>` implementations, then call `services.AddCommandRunners<TCommand>()` to wire them up.
