# Static Analysis for State Machine Configuration

The FunctionalStateMachine library includes comprehensive static analysis to detect common configuration mistakes and anti-patterns during state machine building. Analysis runs automatically when calling `.Build()` and validates the configuration before the state machine is created.

## Table of Contents

1. [Error Detection](#error-detection)
2. [Warning Detection](#warning-detection)
3. [Scenarios and Examples](#scenarios-and-examples)
4. [Performance Considerations](#performance-considerations)
5. [Disabling Analysis](#disabling-analysis)

---

## Error Detection

Errors block state machine creation and must be fixed. They represent definite configuration mistakes.

### 1. Unreachable States

**What it detects:** States that cannot be reached from the initial state through any transition path.

**Why it matters:** Unreachable states are dead code in your state machine. They can never be entered during normal operation, indicating either:
- A forgotten transition from another state
- An incorrectly configured initial state
- States that should be removed

**Example (❌ ERROR):**
```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Start)
    .For(State.Start)
        .On<Trigger.Next>()
            .TransitionTo(State.Processing)
    .For(State.Processing)
        .On<Trigger.Done>()
            .TransitionTo(State.Start)
    .For(State.Abandoned)  // ❌ No transition leads here!
        .On<Trigger.Reset>()
            .TransitionTo(State.Start)
    .Build();  // ❌ Throws: "State 'Abandoned' is unreachable from initial state 'Start'"
```

**Fix:**
```csharp
.For(State.Start)
    .On<Trigger.Abandon>()
        .TransitionTo(State.Abandoned)  // ✅ Now reachable
```

**Note on hierarchical states:** When a parent state has an initial sub-state, the parent is implicitly reachable if any of its children are reachable.

```csharp
// ✅ VALID: Reaching a sub-state makes parent reachable
.For(State.Shopping)  // Parent state
    .StartsWith(State.Browsing)
.For(State.Browsing)  // Sub-state
    .SubStateOf(State.Shopping)
    
// Parent 'Shopping' is reachable if 'Browsing' is reachable
```

---

### 2. Immediate Transition Cycles

**What it detects:** Circular chains of immediate transitions (A→B→C→A) that would cause infinite loops during state entry.

**Why it matters:** Immediate transitions execute automatically on state entry. A cycle would cause the state machine to loop forever trying to resolve the entry, resulting in a stack overflow.

**Example (❌ ERROR):**
```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.A)
    .For(State.A)
        .Immediately()
            .TransitionTo(State.B)
            .Done()
    .For(State.B)
        .Immediately()
            .TransitionTo(State.A)  // ❌ Cycle: A→B→A
            .Done()
    .Build();  // ❌ Throws: "Infinite loop detected in immediate transitions"
```

**Valid patterns:**
```csharp
// ✅ VALID: Forward chain (no cycle)
.For(State.A)
    .Immediately()
        .TransitionTo(State.B)
        .Done()
.For(State.B)
    .Immediately()
        .TransitionTo(State.C)  // Forward, not back
        .Done()
.For(State.C)
    .On<Trigger.Next>()
        .TransitionTo(State.A)  // Normal transition, not immediate

// ✅ VALID: Guarded immediate transition can fail
.For(State.A)
    .Immediately()
        .Guard(data => data.Ready)  // Can fail, so not infinite
        .TransitionTo(State.B)
        .Done()
```

---

### 3. Ambiguous Transitions

**What it detects:** Multiple unguarded transitions from the same state for the same trigger that target different states.

**Why it matters:** This is ambiguous - the state machine cannot determine which transition to take. The first one defined would always execute, making other transitions unreachable and indicating a logic error.

**Example (❌ ERROR):**
```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.A)
    .For(State.A)
        .On<Trigger.Process>()
            .TransitionTo(State.B)     // First definition
            .Execute(() => new Command.LogB())
        .On<Trigger.Process>()
            .TransitionTo(State.C)     // ❌ Ambiguous! Never reachable
            .Execute(() => new Command.LogC())
    .Build();  // ❌ Throws: "State 'A' has ambiguous transitions for trigger 'Process'"
```

**Valid patterns:**
```csharp
// ✅ VALID: Guarded transitions disambiguate
.For(State.A)
    .On<Trigger.Process>()
        .Guard(data => data.Priority == High)
        .TransitionTo(State.B)
    .On<Trigger.Process>()
        .Guard(data => data.Priority == Low)  // Guard distinguishes
        .TransitionTo(State.C)

// ✅ VALID: Multiple transitions to same state (no ambiguity)
.For(State.A)
    .On<Trigger.Process>()
        .TransitionTo(State.B)
    .On<Trigger.Process>()
        .TransitionTo(State.B)  // Same target, no ambiguity
```

---

## Warning Detection

Warnings don't block state machine creation but indicate potential issues. They're logged to Debug output.

### 1. Unused Trigger Types

**What it detects:** Trigger types defined in your trigger hierarchy but never used in any state transition.

**Why it matters:** Unused triggers indicate incomplete configuration or leftover code from refactoring. Removing them can simplify your state machine API.

**Example (⚠️ WARNING):**
```csharp
abstract record Trigger
{
    record Process : Trigger;
    record Complete : Trigger;
    record Cancel : Trigger;     // ⚠️ Never used
    record Retry : Trigger;      // ⚠️ Never used
}

var machine = StateMachine<...>.Create()
    .For(State.Processing)
        .On<Trigger.Process>()
            .TransitionTo(State.Done)
        .On<Trigger.Complete>()
            .TransitionTo(State.Success)
    .Build();  // ⚠️ Logs warnings about Cancel and Retry never being used
```

**Fix:**
```csharp
// Either add transitions for the triggers:
.On<Trigger.Cancel>()
    .TransitionTo(State.Cancelled)

// Or remove them from the trigger hierarchy if not needed
```

### 2. Dead-End States

**What it detects:** States with no outgoing transitions (potential terminal states).

**Why it matters:** These might be intentional (a final success/failure state), but often indicate forgotten transitions or incomplete configuration.

**Example (⚠️ WARNING):**
```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Processing)
    .For(State.Processing)
        .On<Trigger.Complete>()
            .TransitionTo(State.Done)
    .For(State.Done)
        // ⚠️ No outgoing transitions - is this intentional?
    .Build();  // ⚠️ Logs warning: "State 'Done' has no outgoing transitions"
```

**Valid intentional dead-ends:**
```csharp
// ✅ VALID: Terminal success state
.For(State.Completed)
    // Intentionally final, no transitions out

// ✅ VALID: Terminal error state  
.For(State.Failed)
    // Intentionally final

// ✅ VALID: Initial state as terminal is allowed
.StartWith(State.SingleState)
.For(State.SingleState)
    // OK if this is the only state
```

---

## Scenarios and Examples

### Complete Valid State Machine

Here's a real-world example that passes all analysis checks:

```csharp
enum OrderState { Pending, Processing, Shipped, Delivered, Cancelled }
record OrderData(string Id, decimal Total) : data;

// ✅ PASSES all analysis checks
var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .StartWith(OrderState.Pending)
    
    // Initial state: reachable ✓
    .For(OrderState.Pending)
        .On<OrderTrigger.Process>()
            .Guard(data => data.Total > 0)
            .TransitionTo(OrderState.Processing)
        .On<OrderTrigger.Cancel>()
            .TransitionTo(OrderState.Cancelled)
    
    // Processing state: reachable ✓, has outgoing transitions ✓
    .For(OrderState.Processing)
        .On<OrderTrigger.Ship>()
            .TransitionTo(OrderState.Shipped)
        .On<OrderTrigger.Cancel>()
            .Guard(data => DateTime.Now < CutoffTime)  // Guarded, not ambiguous ✓
            .TransitionTo(OrderState.Cancelled)
    
    // Shipped state: reachable ✓, has outgoing transition ✓
    .For(OrderState.Shipped)
        .On<OrderTrigger.Deliver>()
            .TransitionTo(OrderState.Delivered)
    
    // Delivered: reachable ✓, terminal (intentional) ✓
    .For(OrderState.Delivered)
    
    // Cancelled: reachable ✓, terminal (intentional) ✓
    .For(OrderState.Cancelled)
    
    .Build();  // ✅ Success!
```

### Anti-patterns to Avoid

```csharp
// ❌ ANTI-PATTERN 1: Unreachable alternate flow
.For(State.A)
    .On<Trigger.Go>()
        .TransitionTo(State.B)
.For(State.B)
    .On<Trigger.Next>()
        .TransitionTo(State.C)
.For(State.AlternateC)  // Never reached!
    
// ❌ ANTI-PATTERN 2: Ambiguous without guards
.For(State.Processing)
    .On<Trigger.Complete>()
        .TransitionTo(State.Success)
    .On<Trigger.Complete>()
        .TransitionTo(State.Error)    // Which one runs?

// ❌ ANTI-PATTERN 3: Immediate loop
.For(State.A)
    .Immediately()
        .TransitionTo(State.B)
.For(State.B)
    .Immediately()
        .TransitionTo(State.A)        // Stack overflow!
```

---

## Performance Considerations

### When Analysis Runs

Analysis executes during `.Build()`, which typically happens:
- Once at application startup
- During configuration phase
- In tests during setup

It does **NOT** run during:
- `.Start()`
- `.Fire(trigger, ...)`
- Any other state machine operations

### Analysis Complexity

- **Reachability:** O(S + T) where S = states, T = transitions (BFS algorithm)
- **Cycle detection:** O(S + T) (DFS algorithm)
- **Ambiguous transitions:** O(S × T) worst case
- **Dead-end detection:** O(S)

For typical state machines (< 100 states), analysis completes in < 1ms.

### Performance Impact

**On release builds:**
- Zero impact during application runtime
- Build-time cost is negligible (< 1ms per state machine)

**On debug builds:**
- Same negligible cost, but with additional warning logging

---

## Disabling Analysis

Analysis runs by default and is recommended for all builds. However, it can be disabled if needed:

### Method 1: Using SkipAnalysis() (Recommended for Special Cases)

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Initial)
    .For(State.Initial)
        // ... configuration ...
    .SkipAnalysis()  // Disable analysis for this build
    .Build();
```

**When to use:**
- Dynamic state machines (generated at runtime)
- Legacy state machines with known issues being migrated
- Performance-critical situations (though analysis is already < 1ms)

**⚠️ Warning:** Disabling analysis is not recommended for production code.

### Method 2: Conditional Compilation (Release Builds)

To run analysis only in Debug builds:

```csharp
var builder = StateMachine<...>.Create()
    // ... configuration ...
    
#if DEBUG
    // Analysis runs
    .Build()
#else
    // Skip analysis in Release builds
    .SkipAnalysis()
    .Build()
#endif
```

**Pros:**
- Zero overhead in Release builds
- Full validation in Debug

**Cons:**
- Possible configuration errors slip to production
- Not recommended

### Performance Impact

**Recommendation:** Keep analysis enabled in all builds. The cost is negligible (< 1ms) and catches real bugs.

Analysis runs once during `.Build()` which typically happens at:
- Application startup (< 1ms overhead)
- Test setup (negligible)

It does NOT run during:
- `.Start()`
- `.Fire(trigger, ...)`
- Any runtime state operations

## Related Documentation

- [State Machine Guide](./index.md)
- [Conditional Steps Guide](./conditional-steps.md)
- [Immediate Transitions](./index.md#immediate-transitions)
