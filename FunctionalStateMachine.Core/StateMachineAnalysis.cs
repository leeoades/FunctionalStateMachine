using System.Collections.ObjectModel;

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

        // Analyze dead-end states
        AnalyzeDeadEndStates(states, initialState, result);

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
                    if (transition.HasTargetState && !reachable.Contains(transition.TargetState!))
                    {
                        reachable.Add(transition.TargetState!);
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
                            !reachable.Contains(targetDef.InitialSubState))
                        {
                            reachable.Add(targetDef.InitialSubState);
                            toVisit.Enqueue(targetDef.InitialSubState);
                        }
                    }
                }
            }

            // Add immediate transition targets
            foreach (var immediate in definition.ImmediateTransitions)
            {
                if (immediate.HasTargetState && !reachable.Contains(immediate.TargetState!))
                {
                    reachable.Add(immediate.TargetState!);
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
                        !reachable.Contains(immediateDef.InitialSubState))
                    {
                        reachable.Add(immediateDef.InitialSubState);
                        toVisit.Enqueue(immediateDef.InitialSubState);
                    }
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
        if (!reachable.Contains(parentState))
        {
            reachable.Add(parentState);
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

        if (visiting.Contains(state))
        {
            result.AddError($"Infinite loop detected in immediate transitions involving state '{state}'. " +
                "Check for circular immediate transitions that could cause stack overflow.");
            return;
        }

        visiting.Add(state);

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
    /// Detect multiple unguarded transitions for the same trigger (ambiguous routing).
    /// </summary>
    private static void AnalyzeAmbiguousTransitions(
        IReadOnlyDictionary<TState, StateMachine<TState, TTrigger, TData, TCommand>.StateDefinition> states,
        AnalysisResult result)
    {
        foreach (var (state, definition) in states)
        {
            foreach (var (triggerKey, transitions) in definition.Transitions)
            {
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
                        var triggerType = GetTriggerTypeName(triggerKey);
                        result.AddError(
                            $"State '{state}' has ambiguous transitions for trigger '{triggerType}' leading to different states: " +
                            string.Join(", ", targets.Select(t => $"'{t}'")) +
                            ". Add guards or consolidate transitions to resolve the ambiguity.");
                    }
                }
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
        foreach (var (state, definition) in states)
        {
            // Skip initial state - it's okay for it to be a dead-end (terminal state)
            if (state!.Equals(initialState))
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

    private static string GetTriggerTypeName(object triggerKey)
    {
        // triggerKey is typically the trigger type or trigger value
        return triggerKey?.GetType().Name ?? "Unknown";
    }
}
