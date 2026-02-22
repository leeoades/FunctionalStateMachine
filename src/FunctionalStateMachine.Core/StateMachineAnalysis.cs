using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FunctionalStateMachine.Core;

/// <summary>
/// Static analysis warnings and errors for state machine configuration.
/// </summary>
internal sealed class AnalysisResult
{
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];

    public bool IsValid => Errors.Count == 0;

    public void AddError(string message) => Errors.Add(message);
    public void AddWarning(string message) => Warnings.Add(message);
}

/// <summary>
/// Performs static analysis on state machine configuration to detect common issues.
/// </summary>
internal static class StateMachineAnalyzer<TState, TTrigger, TData, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
    /// <summary>
    /// Analyze the state machine configuration for potential issues.
    /// </summary>
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
        Justification = "AnalyzeUnusedTriggers uses reflection to scan for derived trigger types. " +
                        "In trimmed builds, trigger types removed by the trimmer won't be found, " +
                        "so no 'unused trigger' warnings will be emitted for them. This is acceptable " +
                        "since trimmed types are genuinely unused by the application.")]
#endif
    public static AnalysisResult Analyze(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        TState initialState)
    {
        var result = new AnalysisResult();

        if (states.Count == 0)
            return result;

        // Analyze reachability
        AnalyzeReachability(states, initialState, result);

        // Analyze immediate transition cycles
        AnalyzeImmediateTransitionCycles(states, result);

        // Analyze ambiguous transitions
        AnalyzeAmbiguousTransitions(states, result);

        // Analyze conditional transition target ambiguity
        AnalyzeConditionalTransitionTargets(states, result);

        // Analyze dead-end states
        AnalyzeDeadEndStates(states, initialState, result);

        // Analyze unused triggers
        AnalyzeUnusedTriggers(states, result);

        return result;
    }

    /// <summary>
    /// Detect states that cannot be reached from the initial state.
    /// </summary>
    private static void AnalyzeReachability(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        TState initialState,
        AnalysisResult result)
    {
        var reachable = new HashSet<TState>();
        var toVisit = new Queue<TState>();

        toVisit.Enqueue(initialState);
        reachable.Add(initialState);

        // Add initial sub-states to reachable set
        if (states.TryGetValue(initialState, out var initialDef) && initialDef.HasInitialSubState)
        {
            reachable.Add(initialDef.InitialSubState);
            toVisit.Enqueue(initialDef.InitialSubState);
        }

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            if (!states.TryGetValue(current, out var definition))
                continue;

            // Add all transition targets
            foreach (var transitions in definition.Transitions.Values)
            {
                foreach (var transition in transitions)
                {
                    foreach (var target in GetTransitionTargetStates(transition.Steps))
                    {
                        if (reachable.Add(target))
                        {
                            toVisit.Enqueue(target);

                            // Mark parent states as reachable too
                            if (states.TryGetValue(target, out var targetDef) && 
                                targetDef.HasParentState)
                            {
                                MarkParentStatesReachable(targetDef.ParentState, states, reachable, toVisit);
                            }

                            // Add initial sub-states of target
                            if (states.TryGetValue(target, out targetDef) && 
                                targetDef.HasInitialSubState && 
                                reachable.Add(targetDef.InitialSubState))
                            {
                                toVisit.Enqueue(targetDef.InitialSubState);
                            }
                        }
                    }

                    if (transition.HasTargetState && reachable.Add(transition.TargetState!))
                    {
                        toVisit.Enqueue(transition.TargetState!);

                        // Mark parent states as reachable too
                        if (states.TryGetValue(transition.TargetState!, out var targetDef) && 
                            targetDef.HasParentState)
                        {
                            MarkParentStatesReachable(targetDef.ParentState, states, reachable, toVisit);
                        }

                        // Add initial sub-states of target
                        if (states.TryGetValue(transition.TargetState!, out targetDef) && 
                            targetDef.HasInitialSubState && 
                            reachable.Add(targetDef.InitialSubState))
                        {
                            toVisit.Enqueue(targetDef.InitialSubState);
                        }
                    }
                }
            }

            // Add immediate transition targets
            foreach (var immediate in definition.ImmediateTransitions.Where(immediate => immediate.HasTargetState && reachable.Add(immediate.TargetState!)))
            {
                toVisit.Enqueue(immediate.TargetState!);

                // Mark parent states as reachable too
                if (states.TryGetValue(immediate.TargetState!, out var immediateDef) && 
                    immediateDef.HasParentState)
                {
                    MarkParentStatesReachable(immediateDef.ParentState, states, reachable, toVisit);
                }

                // Add initial sub-states of immediate target
                if (states.TryGetValue(immediate.TargetState!, out immediateDef) && 
                    immediateDef.HasInitialSubState && 
                    reachable.Add(immediateDef.InitialSubState))
                {
                    toVisit.Enqueue(immediateDef.InitialSubState);
                }
            }
        }

        // Report unreachable states
        foreach (var state in states.Keys)
        {
            if (!reachable.Contains(state))
            {
                result.AddError($"State '{state}' is unreachable from initial state '{initialState}'");
            }
        }
    }

    private static void MarkParentStatesReachable(
        TState parentState,
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        HashSet<TState> reachable,
        Queue<TState> toVisit)
    {
        if (reachable.Add(parentState))
        {
            toVisit.Enqueue(parentState);
        }

        // Recursively mark grandparent states as reachable
        if (states.TryGetValue(parentState, out var parentDef) && parentDef.HasParentState)
        {
            MarkParentStatesReachable(parentDef.ParentState, states, reachable, toVisit);
        }
    }

    /// <summary>
    /// Detect cycles in immediate transitions that could cause infinite loops.
    /// </summary>
    private static void AnalyzeImmediateTransitionCycles(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        AnalysisResult result)
    {
        var visiting = new HashSet<TState>();
        var visited = new HashSet<TState>();

        foreach (var state in states.Keys)
        {
            if (!visited.Contains(state))
            {
                DetectCycle(state, states, visiting, visited, result);
            }
        }
    }

    private static void DetectCycle(
        TState state,
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        HashSet<TState> visiting,
        HashSet<TState> visited,
        AnalysisResult result)
    {
        if (visited.Contains(state))
            return;

        if (!visiting.Add(state))
        {
            result.AddError($"Infinite loop detected in immediate transitions involving state '{state}'. " +
                "Check for circular immediate transitions that could cause stack overflow.");
            return;
        }

        if (states.TryGetValue(state, out var definition))
        {
            foreach (var immediate in definition.ImmediateTransitions)
            {
                if (immediate.HasTargetState)
                {
                    DetectCycle(immediate.TargetState!, states, visiting, visited, result);
                }
            }
        }

        visiting.Remove(state);
        visited.Add(state);
    }

    /// <summary>
    /// Detect multiple transitions for the same trigger and unreachable transitions.
    /// Uses first-match semantics: only the first matching transition executes.
    /// </summary>
    private static void AnalyzeAmbiguousTransitions(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        AnalysisResult result)
    {
        foreach (var kvp in states)
        {
            var state = kvp.Key;
            var definition = kvp.Value;
            foreach (var transitionKvp in definition.Transitions)
            {
                var triggerKey = transitionKvp.Key;
                var transitions = transitionKvp.Value;
                var triggerType = GetTriggerTypeName(triggerKey);

                // Error: Multiple transitions on the same trigger
                if (transitions.Count > 1)
                {
                    // Check if there's an unguarded transition before the last one
                    for (int i = 0; i < transitions.Count - 1; i++)
                    {
                        if (transitions[i].Guard == null)
                        {
                            result.AddError(
                                $"State '{state}' has an unguarded transition for trigger '{triggerType}' at position {i + 1}, " +
                                "making subsequent transitions unreachable. Only the first matching transition executes (first-match semantics). " +
                                "Either add a guard to this transition or remove subsequent transitions for this trigger.");
                        }
                    }

                    // Warning: Multiple guarded transitions (might be intentional routing, but worth noting)
                    if (transitions.All(t => t.Guard != null))
                    {
                        result.AddWarning(
                            $"State '{state}' has {transitions.Count} guarded transitions for trigger '{triggerType}'. " +
                            "Only the first matching guard will execute (first-match semantics). " +
                            "Ensure guards are ordered from most specific to least specific.");
                    }
                }

                // Find unguarded transitions
                var unguardedTransitions = transitions.Where(t => t.Guard == null).ToList();

                if (unguardedTransitions.Count > 1 && unguardedTransitions.All(t => t.HasTargetState))
                {
                    var targets = unguardedTransitions
                        .Select(t => t.TargetState?.ToString() ?? "null")
                        .Distinct()
                        .ToList();

                    // Error if they go to different states (ambiguous routing)
                    if (targets.Count > 1)
                    {
                        result.AddError(
                            $"State '{state}' has {unguardedTransitions.Count} unguarded transitions for trigger '{triggerType}' " +
                            $"leading to different states: {string.Join(", ", targets.Select(t => $"'{t}'"))}. " +
                            "Add guards or consolidate transitions to resolve the ambiguity.");
                    }
                }
            }
        }
    }

    private static void AnalyzeConditionalTransitionTargets(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        AnalysisResult result)
    {
        foreach (var kvp in states)
        {
            var state = kvp.Key;
            var definition = kvp.Value;
            foreach (var transitionKvp in definition.Transitions)
            {
                var triggerKey = transitionKvp.Key;
                var transitions = transitionKvp.Value;
                foreach (var transition in transitions)
                {
                    var maxTransitionCount = GetMaxTransitionCountPerPath(transition.Steps);
                    if (transition.HasTargetState && maxTransitionCount > 0)
                    {
                        maxTransitionCount++;
                    }

                    if (maxTransitionCount > 1)
                    {
                        var triggerType = GetTriggerTypeName(triggerKey);
                        result.AddError(
                            $"State '{state}' has multiple TransitionTo steps for trigger '{triggerType}'. " +
                            "A single transition may only resolve to one target state.");
                    }
                }
            }
        }
    }

    private static int GetMaxTransitionCountPerPath(
        List<StateMachine<TState, TTrigger, TData, TCommand>.TransitionStep> steps)
    {
        var count = 0;
        foreach (var step in steps)
        {
            switch (step.Kind)
            {
                case StateMachine<TState, TTrigger, TData, TCommand>.TransitionStepKind.Transition:
                    count++;
                    break;
                case StateMachine<TState, TTrigger, TData, TCommand>.TransitionStepKind.ModifyData:
                case StateMachine<TState, TTrigger, TData, TCommand>.TransitionStepKind.Execute:
                    break;
                case StateMachine<TState, TTrigger, TData, TCommand>.TransitionStepKind.ConditionalChain:
                    var maxBranch = 0;
                    foreach (var branch in step.ConditionalBranches!)
                    {
                        maxBranch = Math.Max(maxBranch, GetMaxTransitionCountPerPath(branch.Steps));
                    }
                    count += maxBranch;
                    break;
            }
        }

        return count;
    }

    private static HashSet<TState> GetTransitionTargetStates(
        List<StateMachine<TState, TTrigger, TData, TCommand>.TransitionStep> steps)
    {
        var targets = new HashSet<TState>();
        CollectTransitionTargetStates(steps, targets);
        return targets;
    }

    private static void CollectTransitionTargetStates(
        List<StateMachine<TState, TTrigger, TData, TCommand>.TransitionStep> steps,
        HashSet<TState> targets)
    {
        foreach (var step in steps)
        {
            switch (step.Kind)
            {
                case StateMachine<TState, TTrigger, TData, TCommand>.TransitionStepKind.Transition:
                    targets.Add(step.TargetState!);
                    break;
                case StateMachine<TState, TTrigger, TData, TCommand>.TransitionStepKind.ConditionalChain:
                    foreach (var branch in step.ConditionalBranches!)
                    {
                        CollectTransitionTargetStates(branch.Steps, targets);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Warn about states with no outgoing transitions (potential dead-ends).
    /// </summary>
    private static void AnalyzeDeadEndStates(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        TState initialState,
        AnalysisResult result)
    {
        foreach (var kvp in states)
        {
            var state = kvp.Key;
            var definition = kvp.Value;
            // Skip initial state - it's okay for it to be a dead-end (terminal state)
            if (state.Equals(initialState))
                continue;

            // Check if this state has no outgoing transitions
            var hasOutgoing = definition.Transitions.Values.Any(list => list.Count > 0) ||
                              definition.ImmediateTransitions.Count > 0;

            if (!hasOutgoing)
            {
                result.AddWarning(
                    $"State '{state}' has no outgoing transitions. This might be intentional (a terminal state), " +
                    "but verify it's not an oversight.");
            }
        }
    }

    /// <summary>
    /// Detect trigger types that are defined but never used in any transition.
    /// </summary>
#if NET8_0_OR_GREATER
    [RequiresUnreferencedCode("Uses reflection to discover derived trigger types in the assembly")]
#endif
    private static void AnalyzeUnusedTriggers(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        AnalysisResult result)
    {
        // Collect all used trigger types
        var usedTriggers = new HashSet<object>();
        
        foreach (var definition in states.Values)
        {
            foreach (var triggerKey in definition.Transitions.Keys)
            {
                usedTriggers.Add(triggerKey);
            }
        }

        // Get all possible trigger types from the TTrigger type
        var triggerType = typeof(TTrigger);
        
        // If TTrigger is a sealed record hierarchy, find all derived types
        var allTriggerTypes = GetAllTriggerTypes(triggerType);
        
        // Find unused trigger types
        foreach (var possibleTrigger in allTriggerTypes)
        {
            // Check if this trigger type is used
            bool isUsed = usedTriggers.Any(t => 
            {
                // Handle both type-based and value-based triggers
                if (t is Type)
                    return (Type)t == possibleTrigger;
                return t.GetType() == possibleTrigger;
            });

            if (!isUsed)
            {
                result.AddWarning(
                    $"Trigger type '{possibleTrigger.Name}' is defined but never used in any transition. " +
                    "Consider removing it or adding transitions that use it.");
            }
        }
    }

#if NET8_0_OR_GREATER
    [RequiresUnreferencedCode("Uses Assembly.GetTypes() which may not return all types in trimmed builds")]
#endif
    private static HashSet<Type> GetAllTriggerTypes(Type triggerType)
    {
        var types = new HashSet<Type>();
        
        // Add the base trigger type if it's abstract/record
        if (triggerType.IsAbstract || IsRecordType(triggerType))
        {
            // Find all derived types in the same assembly
            var assembly = triggerType.Assembly;
            foreach (var type in assembly.GetTypes())
            {
                // Check if it's a record/sealed record deriving from the trigger type
                if (type != triggerType && 
                    triggerType.IsAssignableFrom(type) && 
                    !type.IsAbstract &&
                    IsRecordType(type))
                {
                    types.Add(type);
                }
            }
        }
        
        // Always add the trigger type itself as a fallback
        if (types.Count == 0)
        {
            types.Add(triggerType);
        }

        return types;
    }

#if NET8_0_OR_GREATER
    [RequiresUnreferencedCode("Uses Type.GetProperty() to inspect non-public properties, which may be removed by trimming")]
#endif
    private static bool IsRecordType(Type type)
    {
        // Records are detected by checking for the generated 'EqualityContract' property
        return type.GetProperty("EqualityContract", 
            BindingFlags.NonPublic | BindingFlags.Instance) != null;
    }

    private static string GetTriggerTypeName(object triggerKey)
    {
        // triggerKey is typically the trigger type or trigger value
        return triggerKey.GetType().Name;
    }
}
