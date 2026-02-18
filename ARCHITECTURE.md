# Architecture

This document provides a technical deep-dive into the Functional State Machine architecture, design decisions, and implementation details.

## Table of Contents

- [Core Principles](#core-principles)
- [Type System](#type-system)
- [State Machine Lifecycle](#state-machine-lifecycle)
- [Builder Pattern](#builder-pattern)
- [Transition Execution](#transition-execution)
- [Hierarchical States](#hierarchical-states)
- [Static Analysis](#static-analysis)
- [Command Dispatching](#command-dispatching)
- [Performance Considerations](#performance-considerations)

## Core Principles

### 1. Pure Functions

The state machine's `Fire()` method is a pure function:

```csharp
(TState newState, TData newData, IReadOnlyList<TCommand> commands) = 
    machine.Fire(trigger, currentState, currentData);
```

**Input**: Trigger, current state, current data  
**Output**: New state, new data, commands to execute  
**No Side Effects**: No I/O, no mutations, deterministic

This enables:
- **Testability**: Assert on returned values without mocks
- **Determinism**: Same inputs always produce same outputs
- **Replayability**: Store events and replay transitions
- **Debugging**: Step through logic without side effects

### 2. Immutability

All state machine components are immutable:

- **State Machine**: Built once, never modified
- **State Data**: Use `with` expressions for updates
- **Commands**: Immutable records describing intent
- **Triggers**: Immutable records carrying data

### 3. Separation of Concerns

```
┌─────────────────┐
│  State Machine  │  ← Logical rules & transitions
└────────┬────────┘
         │ returns
         ▼
    ┌──────────┐
    │ Commands │     ← What should happen
    └────┬─────┘
         │ dispatched to
         ▼
┌─────────────────┐
│ Command Runners │  ← How to execute
└─────────────────┘
```

The state machine defines **business logic**.  
Command runners handle **infrastructure**.

## Type System

### Four Generic Parameters

```csharp
StateMachine<TState, TTrigger, TData, TCommand>
```

| Type | Purpose | Typical Structure |
|------|---------|-------------------|
| `TState` | State identifier | `enum` or record |
| `TTrigger` | Trigger hierarchy | `abstract record` with sealed subtypes |
| `TData` | State-associated data | `sealed record` |
| `TCommand` | Command hierarchy | `abstract record` with sealed subtypes |

### Trigger Hierarchy Pattern

```csharp
public abstract record PaymentTrigger
{
    public sealed record StartPayment(decimal Amount) : PaymentTrigger;
    public sealed record CardAuthorized(string TransactionId) : PaymentTrigger;
    public sealed record PaymentFailed(string Reason) : PaymentTrigger;
}
```

**Benefits**:
- Type-safe trigger matching with `On<TTrigger>()`
- Compile-time verification of trigger types
- Data can be carried with triggers
- Pattern matching for trigger handling

### Command Hierarchy Pattern

```csharp
public abstract record PaymentCommand
{
    public sealed record ChargeCard(decimal Amount) : PaymentCommand;
    public sealed record SendReceipt(string Email) : PaymentCommand;
    public sealed record LogError(string Message) : PaymentCommand;
}
```

Same benefits as triggers, plus:
- Clear intent of what should happen
- Testable without executing effects
- Can be persisted for audit trails

## State Machine Lifecycle

### 1. Configuration Phase (Build Time)

```csharp
var builder = StateMachine<TState, TTrigger, TData, TCommand>.Create()
    .StartWith(initialState)
    .For(state)
        .On<SomeTrigger>()
            .TransitionTo(nextState)
            .Execute(() => command)
    // ... more configuration
    .Build();  // ← Validation happens here
```

**Build Process**:
1. Collect state configurations
2. Validate completeness
3. Run static analysis
4. Generate internal lookup structures
5. Return immutable state machine

### 2. Execution Phase (Runtime)

```csharp
var (newState, newData, commands) = 
    machine.Fire(trigger, currentState, currentData);
```

**Fire Process**:
1. Find matching transition for (state, trigger)
2. Evaluate guards if present
3. Execute exit commands for current state
4. Modify data if specified
5. Execute transition commands
6. Execute entry commands for new state
7. Process immediate transitions
8. Return tuple of (state, data, commands)

## Builder Pattern

### Fluent Configuration

The builder uses a sophisticated type system to provide IntelliSense guidance:

```csharp
public class StateConfiguration<TState, TTrigger, TData, TCommand>
{
    public TransitionConfiguration<...> On<T>() where T : TTrigger;
    public StateConfiguration<...> OnEntry(...);
    public StateConfiguration<...> OnExit(...);
    public StateConfiguration<...> Immediately(...);
    public StateConfiguration<...> WithInitialSubState(...);
}

public class TransitionConfiguration<...>
{
    public TransitionConfiguration<...> TransitionTo(TState state);
    public TransitionConfiguration<...> ModifyData(Func<TData, TData> modifier);
    public TransitionConfiguration<...> Execute(Func<TCommand> command);
    public TransitionConfiguration<...> Guard(Func<bool> condition);
    public ConditionalConfiguration<...> If(Func<bool> condition);
}
```

Each configuration method returns a specific type that exposes only valid next operations.

### Method Chaining Rules

```csharp
.For(State)
    .On<Trigger>()           // Start transition configuration
        .Guard(() => ...)     // Guards before transition
        .If(() => ...)        // Conditional steps
            .Execute(...)     // Commands in branch
            .TransitionTo()   // Transition in branch
        .ElseIf(() => ...)
            .Execute(...)
        .Else()
            .Execute(...)
        .Done()               // End conditional block
        .ModifyData(...)      // Data modification
        .Execute(...)         // Unconditional commands
        .TransitionTo(...)    // Final transition
```

**Order Constraints**:
- Guards must come before conditionals
- Conditionals must be closed with `Done()`
- `TransitionTo()` can appear in conditionals or at end
- `ModifyData()` affects subsequent commands

## Transition Execution

### Execution Order

```
1. Guards (evaluate conditions)
   ↓
2. Conditional Steps (if guards pass)
   ↓
3. Data Modification
   ↓
4. Transition Commands
   ↓
5. Exit Commands (current state)
   ↓
6. Entry Commands (new state)
   ↓
7. Immediate Transitions (if configured)
```

### Guard Evaluation

Guards follow **first-match semantics**:

```csharp
.For(State)
    .On<Trigger>()
        .Guard(() => condition1)  // ← Evaluated first
        .TransitionTo(StateA)
    .On<Trigger>()
        .Guard(() => condition2)  // ← Only if condition1 is false
        .TransitionTo(StateB)
    .On<Trigger>()                // ← Catch-all (no guard)
        .TransitionTo(StateC)
```

**Important**: Order matters! The first matching transition wins.

### Command Collection

Commands are collected in order:

```csharp
.For(State)
    .OnExit(() => new ExitCommand())
    .On<Trigger>()
        .Execute(() => new TransitionCommand1())
        .Execute(() => new TransitionCommand2())
        .TransitionTo(NextState)
    .For(NextState)
        .OnEntry(() => new EntryCommand())
```

Results in: `[TransitionCommand1, TransitionCommand2, ExitCommand, EntryCommand]`

## Hierarchical States

### Parent-Child Relationships

```csharp
.For(ParentState)
    .WithInitialSubState(ChildState1)
    .On<SomeGlobalTrigger>()
        .TransitionTo(AnotherState)
        
.For(ChildState1)
    .SubStateOf(ParentState)
    .On<ChildSpecificTrigger>()
        .TransitionTo(ChildState2)
```

### Trigger Propagation

When a trigger fires in a child state:

1. **Check child state** for matching transition
2. **If not found**, check parent state
3. **If not found**, propagate to parent's parent
4. **If still not found**, trigger is unhandled

### Entry/Exit with Hierarchies

```
Transition: ChildA → ChildB (different parents)

1. Exit ChildA
2. Exit ParentA
3. Enter ParentB
4. Enter ChildB
```

The library automatically manages entry/exit order.

## Static Analysis

### Build-Time Validation

The `.Build()` method performs several checks internally to catch configuration errors early:

#### 1. Reachability Analysis

Detects states that can never be reached from the initial state using breadth-first search. The build will warn about unreachable states that may indicate missing transitions or configuration errors.

**What you'll see**: Warning about unreachable states in your state machine configuration.

#### 2. Cycle Detection

Detects immediate transition cycles where states transition to themselves or form circular chains without any trigger, which would cause infinite loops.

**What you'll see**: Error indicating a circular immediate transition chain was detected.

#### 3. Ambiguous Transitions

Checks for multiple unguarded transitions on the same trigger from the same state, which would make the behavior ambiguous.

**What you'll see**: Error indicating multiple unguarded transitions exist for the same trigger in a state.

#### 4. Guard Ordering

Warns if unguarded transitions appear before guarded transitions on the same trigger, which makes the guarded transitions unreachable due to first-match semantics.

**What you'll see**: Error indicating an unguarded transition makes subsequent guarded transitions unreachable.

### Opt-Out

```csharp
.SkipAnalysis()  // Disable all static analysis checks
```

## Command Dispatching

### Manual Dispatch

```csharp
var (_, _, commands) = machine.Fire(trigger, state, data);

foreach (var command in commands)
{
    switch (command)
    {
        case ChargeCard cmd:
            await paymentService.ChargeAsync(cmd.Amount);
            break;
        // ... more cases
    }
}
```

### Source Generated Dispatch

```csharp
// 1. Define runners
public class ChargeCardRunner : ICommandRunner<ChargeCard>
{
    public async Task RunAsync(ChargeCard command)
    {
        await paymentService.ChargeAsync(command.Amount);
    }
}

// 2. Register runners
services.AddCommandRunners<PaymentCommand>();

// 3. Dispatch automatically
var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher<PaymentCommand>>();
await dispatcher.DispatchAsync(commands);
```

The source generator creates:

```csharp
// Generated code
public class CommandDispatcher : ICommandDispatcher<PaymentCommand>
{
    public async Task DispatchAsync(IEnumerable<PaymentCommand> commands)
    {
        foreach (var command in commands)
        {
            switch (command)
            {
                case ChargeCard cmd:
                    await _chargeCardRunner.RunAsync(cmd);
                    break;
                // ... generated cases
            }
        }
    }
}
```

## Performance Considerations

### Memory Allocation

**Zero Allocation After Build**:
- State machine structure is built once
- `Fire()` only allocates for the command list
- Uses `struct` internally where possible

**Data Efficiency**:
- Record types use structural equality
- `with` expressions minimize allocations
- Commands are lightweight records

### Lookup Performance

**Transition Lookup**: O(1)
```csharp
// Internal structure
Dictionary<(TState, Type), TransitionDefinition>
```

**State Configuration**: O(1)
```csharp
Dictionary<TState, StateDefinition>
```

### Static Analysis Cost

- **When**: Only during `.Build()`
- **Cost**: < 1ms per state machine (typical)
- **Trade-off**: Upfront cost for runtime safety

### Benchmark Results

```
| Method          | States | Transitions | Time    | Allocated |
|-----------------|--------|-------------|---------|-----------|
| Build           | 10     | 20          | 150 μs  | 15 KB     |
| Fire (simple)   | -      | -           | 50 ns   | 80 B      |
| Fire (complex)  | -      | -           | 150 ns  | 240 B     |
```

## Design Decisions

### Why Records?

**Triggers and Commands**:
- Immutable by default
- Structural equality for free
- Concise syntax
- Pattern matching support

### Why Not Reflection at Runtime?

- **Performance**: Reflection is slow
- **AOT Compatible**: Works with native AOT
- **Type Safety**: Compile-time checks

Instead, we use:
- Generic type parameters
- Source generation (for dispatchers)
- Static analysis at build time

### Why Separate Command Execution?

**Alternative Approach** (side effects in transitions):
```csharp
.On<PaymentTrigger>()
    .Execute(() => database.Save(...))  // ❌ Side effect
```

**Our Approach** (commands returned):
```csharp
.On<PaymentTrigger>()
    .Execute(() => new SaveToDatabase(...))  // ✅ Pure
```

**Benefits**:
1. **Testability**: Assert on commands without database
2. **Replayability**: Store and replay transitions
3. **Flexibility**: Choose when/how to execute
4. **Transactionality**: Batch commands in a transaction

### Why netstandard2.0?

**Compatibility over Features**:
- Supports .NET Framework 4.6.1+
- Works in Unity, Xamarin, UWP
- Maximum reach for a library

**Polyfills via PolySharp**:
- Modern C# features (records, init)
- No runtime dependency
- Compile-time transformation

## Extension Points

### Custom State Types

```csharp
public record CustomState(string Name, int Priority);

var machine = StateMachine<CustomState, ...>.Create()
    .StartWith(new CustomState("Initial", 0))
    // ...
```

### Custom Analysis Rules

Extend `StateMachineAnalyzer`:

```csharp
public class CustomAnalyzer : StateMachineAnalyzer
{
    protected override void Analyze()
    {
        base.Analyze();
        // Add custom checks
    }
}
```

## Future Architecture

Potential architectural enhancements being considered:

- Parallel states (orthogonal regions)
- History states (deep and shallow)
- State machine composition
- Compiled state machines (AOT)

---

*Last Updated: February 2026*
