# Troubleshooting Guide

This guide helps you diagnose and fix common issues when using Functional State Machine.

## Table of Contents

- [Build Errors](#build-errors)
- [Runtime Errors](#runtime-errors)
- [Configuration Issues](#configuration-issues)
- [Performance Issues](#performance-issues)
- [Command Execution Issues](#command-execution-issues)
- [Diagram Generation Issues](#diagram-generation-issues)

## Build Errors

### "Initial state must be set"

**Error**:
```
InvalidOperationException: Initial state must be set using StartWith()
```

**Cause**: You didn't call `StartWith()` before `Build()`.

**Fix**:
```csharp
var machine = StateMachine<MyState, MyTrigger, MyCommand>.Create()
    .StartWith(MyState.Initial)  // ← Add this
    .Build();
```

---

### "State X is referenced but not configured"

**Error**:
```
InvalidOperationException: State 'Active' is referenced in transitions but never configured with For()
```

**Cause**: You used a state in `TransitionTo()` but never called `.For(state)`.

**Fix**:
```csharp
.For(MyState.Initial)
    .On<StartTrigger>()
        .TransitionTo(MyState.Active)  // Active is referenced
.For(MyState.Active)  // ← Must configure Active too
    .On<StopTrigger>()
        .TransitionTo(MyState.Initial)
```

---

### "Unguarded transition before other transitions"

**Error**:
```
InvalidOperationException: Unguarded transition on 'ProcessTrigger' in state 'Active' 
makes subsequent guarded transitions unreachable
```

**Cause**: An unguarded transition comes before guarded ones, making them unreachable.

**Problem**:
```csharp
.For(MyState.Active)
    .On<ProcessTrigger>()
        .TransitionTo(MyState.Done)      // ← No guard, always matches
    .On<ProcessTrigger>()
        .Guard(() => condition)
        .TransitionTo(MyState.Pending)   // ← Never reached!
```

**Fix**: Put guarded transitions first:
```csharp
.For(MyState.Active)
    .On<ProcessTrigger>()
        .Guard(() => condition)
        .TransitionTo(MyState.Pending)   // ← Checked first
    .On<ProcessTrigger>()
        .TransitionTo(MyState.Done)      // ← Fallback
```

---

### "Circular immediate transition detected"

**Error**:
```
InvalidOperationException: Circular immediate transition chain detected: A → B → C → A
```

**Cause**: Immediate transitions form an infinite loop.

**Problem**:
```csharp
.For(StateA)
    .Immediately(() => true)
    .TransitionTo(StateB)
.For(StateB)
    .Immediately(() => true)
    .TransitionTo(StateC)
.For(StateC)
    .Immediately(() => true)
    .TransitionTo(StateA)  // ← Back to A!
```

**Fix**: Break the cycle or add guards:
```csharp
.For(StateC)
    .Immediately((data) => data.ShouldLoop)  // ← Add condition
    .TransitionTo(StateA)
```

## Runtime Errors

### "No transition found for trigger"

**Error**:
```
InvalidOperationException: No transition found for trigger 'PaymentFailed' in state 'Processing'
```

**Cause**: You fired a trigger that the current state doesn't handle.

**Debugging**:
```csharp
// Check if state has the transition
var canHandle = machine.CanFire<PaymentFailed>(currentState);
if (!canHandle)
{
    // Handle unhandled trigger
}
```

**Fix Option 1**: Add the transition:
```csharp
.For(MyState.Processing)
    .On<PaymentFailed>()  // ← Add missing transition
        .TransitionTo(MyState.Failed)
```

**Fix Option 2**: Use `OnUnhandled`:
```csharp
.For(MyState.Processing)
    .OnUnhandled((trigger) => new LogWarning($"Unhandled: {trigger}"))
```

**Fix Option 3**: Use parent state (hierarchical):
```csharp
.For(ParentState)
    .On<PaymentFailed>()  // ← Handles for all children
        .TransitionTo(MyState.Failed)
        
.For(MyState.Processing)
    .SubStateOf(ParentState)  // ← Inherits parent transitions
```

---

### "Multiple transitions match this trigger"

**Error**:
```
InvalidOperationException: Multiple unguarded transitions found for trigger 'Start' in state 'Idle'
```

**Cause**: Two or more unguarded transitions for same trigger/state.

**Problem**:
```csharp
.For(MyState.Idle)
    .On<StartTrigger>()
        .TransitionTo(MyState.Active)
    .On<StartTrigger>()  // ← Duplicate!
        .TransitionTo(MyState.Pending)
```

**Fix**: Use guards to disambiguate:
```csharp
.For(MyState.Idle)
    .On<StartTrigger>()
        .Guard((data) => data.IsReady)
        .TransitionTo(MyState.Active)
    .On<StartTrigger>()
        .Guard((data) => !data.IsReady)
        .TransitionTo(MyState.Pending)
```

---

### "Guard threw an exception"

**Error**:
```
InvalidOperationException: Guard evaluation threw an exception
Inner: NullReferenceException
```

**Cause**: Guard condition threw an exception during evaluation.

**Problem**:
```csharp
.Guard((data) => data.User.IsActive)  // ← data.User might be null
```

**Fix**: Use null-safe guards:
```csharp
.Guard((data) => data.User?.IsActive == true)
```

Or handle it explicitly:
```csharp
.Guard((data) => {
    try {
        return data.User.IsActive;
    }
    catch {
        return false;  // Default to false on error
    }
})
```

## Configuration Issues

### Guards not working as expected

**Problem**: Guards always evaluate to false or true unexpectedly.

**Common Mistakes**:

```csharp
// ❌ Wrong: Evaluates immediately during build
.Guard(() => DateTime.Now.Hour > 9)

// ✅ Right: Evaluates during Fire()
.Guard(() => DateTime.Now.Hour > 9)  // This is actually correct

// But if using data:
// ❌ Wrong: Capturing wrong data
var capturedData = data;
.Guard(() => capturedData.IsReady)

// ✅ Right: Use data parameter
.Guard((data) => data.IsReady)
```

**Debugging Guards**:
```csharp
.Guard((data) => {
    var result = data.IsReady && data.Count > 0;
    Console.WriteLine($"Guard evaluated: {result}");
    return result;
})
```

---

### Data not updating

**Problem**: State changes but data stays the same.

**Cause**: Forgot to use `ModifyData()`.

```csharp
// ❌ Wrong: No data modification
.On<IncrementTrigger>()
    .TransitionTo(MyState.Active)
// Data is unchanged!

// ✅ Right: Modify data
.On<IncrementTrigger>()
    .ModifyData(data => data with { Count = data.Count + 1 })
    .TransitionTo(MyState.Active)
```

---

### Entry/Exit commands not executing

**Problem**: `OnEntry()` or `OnExit()` commands aren't in the result.

**Cause**: Multiple possible issues:

1. **Forgot to configure entry/exit**:
```csharp
.For(MyState.Active)
    .OnEntry(() => new LogEntry("Entered Active"))  // ← Need this
```

2. **Using internal transition**:
```csharp
// No entry/exit for internal transitions!
.On<SelfTrigger>()
    // No TransitionTo() = internal transition
```

3. **Wrong state**:
```csharp
// Entry/exit is on StateA, but transitioning to StateB
```

## Performance Issues

### Slow Build() time

**Problem**: `.Build()` takes several seconds.

**Causes**:
1. **Static analysis overhead**: Rare, but possible with very large state machines
2. **Too many states/transitions**: 1000+ states

**Solutions**:

```csharp
// Skip analysis if you're confident
.SkipAnalysis()
.Build();

// Or optimize state machine design:
// - Use data instead of state explosion
// - Use hierarchical states to reduce complexity
```

---

### Slow Fire() time

**Problem**: `Fire()` is slower than expected.

**Benchmarking**:
```csharp
var sw = Stopwatch.StartNew();
var (newState, newData, commands) = machine.Fire(trigger, state, data);
sw.Stop();
Console.WriteLine($"Fire took: {sw.Elapsed.TotalMilliseconds}ms");
```

**Common Causes**:

1. **Expensive guards**:
```csharp
// ❌ Slow guard
.Guard(() => Database.CheckSomething())  // I/O in guard!

// ✅ Fast guard
.Guard((data) => data.IsApproved)  // Just check data
```

2. **Too many commands**:
```csharp
// ❌ Many commands
.ExecuteSteps(() => Enumerable.Range(1, 1000).Select(i => new Command(i)))

// ✅ Batch operations
.Execute(() => new BatchCommand(1, 1000))
```

3. **Data modification overhead**:
```csharp
// ❌ Complex modification
.ModifyData(data => ExpensiveOperation(data))

// ✅ Simple modification
.ModifyData(data => data with { Counter = data.Counter + 1 })
```

## Command Execution Issues

### Command runner not found

**Error**:
```
InvalidOperationException: No command runner registered for command 'MyCommand'
```

**Fix**:

```csharp
// 1. Create the runner
public class MyCommandRunner : ICommandRunner<MyCommand>
{
    public Task RunAsync(MyCommand command) { ... }
}

// 2. Register it
services.AddCommandRunners<MyCommand>();
```

---

### Commands executing in wrong order

**Problem**: Commands execute out of order.

**Cause**: Async execution without proper ordering.

**Fix**:
```csharp
// ❌ Wrong: Parallel execution
foreach (var command in commands)
{
    _ = Task.Run(() => dispatcher.DispatchAsync(command));
}

// ✅ Right: Sequential execution
foreach (var command in commands)
{
    await dispatcher.DispatchAsync(command);
}
```

---

### Command execution fails but state already changed

**Problem**: Command fails but state machine already moved to next state.

**This is by design**: State machine is pure and doesn't know about execution failure.

**Solution**: Implement compensation:

```csharp
var (newState, newData, commands) = machine.Fire(trigger, currentState, currentData);

try
{
    await ExecuteCommandsAsync(commands);
    
    // Success: save new state
    await SaveStateAsync(newState, newData);
}
catch (Exception ex)
{
    // Failure: keep old state
    await SaveStateAsync(currentState, currentData);
    
    // Optionally fire compensation trigger
    var (compensatedState, compensatedData, _) = 
        machine.Fire(new CompensationTrigger(ex), currentState, currentData);
}
```

## Diagram Generation Issues

### Diagram doesn't show all states

**Cause**: Unreachable states aren't shown by default.

**Fix**: Ensure all states are reachable or configure them anyway:
```csharp
.For(UnreachableState)  // Configure it even if unreachable
```

---

### Diagram is too large

**Problem**: Generated Mermaid diagram is unreadable.

**Solutions**:

1. **Break into sub-machines**: Split large state machine into multiple smaller ones
2. **Use subgraphs**: Leverage hierarchical states for better organization
3. **Simplify**: Combine similar states using data instead

## Getting Help

If you're still stuck:

1. **Check the docs**: [docs/](docs/)
2. **Review samples**: [samples/](samples/)
3. **Search issues**: [GitHub Issues](https://github.com/leeoades/FunctionalStateMachine/issues)
4. **Ask for help**: Open a new issue with:
   - Your state machine configuration
   - Steps to reproduce
   - Expected vs actual behavior
   - .NET and library versions

## Common Patterns

### Safe Trigger Handling

```csharp
public async Task HandleTriggerSafely<T>(T trigger) where T : MyTrigger
{
    try
    {
        var (newState, newData, commands) = machine.Fire(trigger, currentState, currentData);
        await ExecuteCommandsAsync(commands);
        await SaveStateAsync(newState, newData);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("No transition"))
    {
        // Unhandled trigger
        logger.LogWarning($"Unhandled trigger: {trigger}");
    }
}
```

### Validating State Before Fire

```csharp
public void ValidateState()
{
    // Load state from storage
    var (state, data) = LoadState();
    
    // Validate before using
    if (!Enum.IsDefined(typeof(MyState), state))
    {
        throw new InvalidStateException($"Invalid state: {state}");
    }
    
    // Optionally validate data
    if (data == null || data.IsInvalid())
    {
        throw new InvalidDataException("State data is invalid");
    }
}
```

---

*Can't find your issue? Open a [GitHub issue](https://github.com/leeoades/FunctionalStateMachine/issues/new/choose) and we'll help!*
