using System.Collections.ObjectModel;

namespace FunctionalStateMachine.Core;

public sealed class StateMachine<TState, TTrigger, TData, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
    public static StateMachineBuilder<TState, TTrigger, TData, TCommand> Create()
    {
        return new StateMachineBuilder<TState, TTrigger, TData, TCommand>();
    }

    private readonly Dictionary<TState, StateDefinition> _states = new();
    private Func<TTrigger, TState, IEnumerable<TCommand>>? _onUnhandled;
    private bool _hasInitialState;
    private TState? _initialState;

    internal StateMachine<TState, TTrigger, TData, TCommand> StartWith(TState state)
    {
        _hasInitialState = true;
        _initialState = state;
        return this;
    }

    public (TState State, TData Data) CreateState(TState state, TData data)
        => (ResolveInitialLeaf(state), data);

    internal StateConfiguration For(TState state)
    {
        var definition = GetOrCreateState(state);
        return new StateConfiguration(this, definition);
    }

    internal StateMachine<TState, TTrigger, TData, TCommand> OnUnhandled(
        Func<TTrigger, TState, IEnumerable<TCommand>> handler)
    {
        _onUnhandled = handler;
        return this;
    }

    public (TState NewState, TData NewData, IReadOnlyList<TCommand> Commands) Fire(
        TTrigger trigger,
        TState currentState,
        TData currentData)
    {
        if (TryFireInternal(
            trigger,
            currentState,
            currentData,
            out var newState,
            out var newData,
            out var commands,
            throwOnUnhandled: true))
        {
            return (newState, newData, commands);
        }

        throw new InvalidOperationException("Unhandled trigger.");
    }

    public bool TryFire(
        TTrigger trigger,
        TState currentState,
        TData currentData,
        out TState newState,
        out TData newData,
        out IReadOnlyList<TCommand> commands)
    {
        return TryFireInternal(
            trigger,
            currentState,
            currentData,
            out newState,
            out newData,
            out commands,
            throwOnUnhandled: false);
    }

    public TState InitialState
    {
        get
        {
            if (!_hasInitialState)
            {
                throw new InvalidOperationException("No initial state has been configured.");
            }

            return ResolveInitialLeaf(_initialState!);
        }
    }

    public (TState State, TData Data, IReadOnlyList<TCommand> Commands) Start(TData data)
    {
        var targetState = InitialState;
        var commandList = new List<TCommand>();
        AppendInitialEntryCommands(commandList, targetState, data);
        var (finalState, finalData) = ApplyImmediateTransitions(commandList, targetState, data);

        IReadOnlyList<TCommand> commands = commandList.Count == 0
            ? Array.Empty<TCommand>()
            : new ReadOnlyCollection<TCommand>(commandList);

        return (finalState, finalData, commands);
    }


    private bool TryFireInternal(
        TTrigger trigger,
        TState currentState,
        TData currentData,
        out TState newState,
        out TData newData,
        out IReadOnlyList<TCommand> commands,
        bool throwOnUnhandled)
    {
        if (!_states.TryGetValue(currentState, out var definition))
        {
            throw new InvalidOperationException($"State '{currentState}' is not configured.");
        }

        if (definition.HasChildren)
        {
            throw new InvalidOperationException(
                $"State '{currentState}' is a parent state. Use a leaf state instead.");
        }

        if (!TryGetTransitionsInHierarchy(currentState, trigger, out var transitions))
        {
            return HandleUnhandled(trigger, currentState, currentData, out newState, out newData, out commands,
                throwOnUnhandled);
        }

        foreach (var transition in transitions)
        {
            if (transition.Guard != null && !transition.Guard(currentState, currentData, trigger))
            {
                continue;
            }

            if (transition.IsIgnored)
            {
                newState = currentState;
                newData = currentData;
                commands = [];
                return true;
            }

            var commandList = new List<TCommand>();
            var updatedData = ApplyTransitionSteps(
                commandList,
                transition.Steps,
                currentState,
                currentData,
                trigger,
                out var conditionalHasTargetState,
                out var conditionalTargetState);
            var hasTargetState = transition.HasTargetState || conditionalHasTargetState;
            var targetState = transition.HasTargetState
                ? transition.TargetState!
                : conditionalHasTargetState ? conditionalTargetState : currentState;
            targetState = ResolveInitialLeaf(targetState);
            var isStateChange = hasTargetState
                && !EqualityComparer<TState>.Default.Equals(currentState, targetState);

            if (isStateChange)
            {
                AppendExitCommands(commandList, currentState, targetState, updatedData);
                AppendEntryCommands(commandList, currentState, targetState, updatedData);
                (targetState, updatedData) = ApplyImmediateTransitions(commandList, targetState, updatedData);
            }

            newState = targetState;
            newData = updatedData;
            commands = commandList.Count == 0 ? Array.Empty<TCommand>() : new ReadOnlyCollection<TCommand>(commandList);
            return true;
        }

        return HandleUnhandled(trigger, currentState, currentData, out newState, out newData, out commands,
            throwOnUnhandled);
    }

    private bool HandleUnhandled(
        TTrigger trigger,
        TState currentState,
        TData currentData,
        out TState newState,
        out TData newData,
        out IReadOnlyList<TCommand> commands,
        bool throwOnUnhandled)
    {
        if (_onUnhandled != null)
        {
            var commandList = _onUnhandled(trigger, currentState)?.ToList() ?? new List<TCommand>();
            newState = currentState;
            newData = currentData;
            commands = commandList.Count == 0 ? Array.Empty<TCommand>() : new ReadOnlyCollection<TCommand>(commandList);
            return true;
        }

        if (throwOnUnhandled)
        {
            throw new InvalidOperationException($"Unhandled trigger '{trigger}' in state '{currentState}'.");
        }

        newState = currentState;
        newData = currentData;
        commands = [];
        return false;
    }

    internal void Validate(bool skipAnalysis = false)
    {
        foreach (var definition in _states.Values)
        {
            definition.HasChildren = false;
        }

        foreach (var definition in _states.Values)
        {
            if (!definition.HasParentState)
            {
                continue;
            }

            if (!_states.TryGetValue(definition.ParentState, out var parentDefinition))
            {
                throw new InvalidOperationException(
                    $"State '{definition.State}' declares parent '{definition.ParentState}', but it is not configured.");
            }

            parentDefinition.HasChildren = true;
        }

        foreach (var definition in _states.Values)
        {
            if (definition.HasChildren)
            {
                if (!definition.HasInitialSubState)
                {
                    throw new InvalidOperationException(
                        $"State '{definition.State}' has child states but no initial sub-state configured. Call StartsWith.");
                }

                if (!_states.TryGetValue(definition.InitialSubState, out var initialDefinition))
                {
                    throw new InvalidOperationException(
                        $"State '{definition.State}' starts with '{definition.InitialSubState}', but it is not configured.");
                }

                if (!initialDefinition.HasParentState
                    || !EqualityComparer<TState>.Default.Equals(initialDefinition.ParentState, definition.State))
                {
                    throw new InvalidOperationException(
                        $"State '{definition.State}' starts with '{definition.InitialSubState}', which is not a direct child.");
                }
            }
            else if (definition.HasInitialSubState)
            {
                throw new InvalidOperationException(
                    $"State '{definition.State}' declares StartsWith but has no child states.");
            }

            ValidateNoCycles(definition.State);
        }

        foreach (var definition in _states.Values)
        {
            foreach (var transition in definition.GetTransitions())
            {
                if (!transition.HasTargetState && !HasTransitionTargetSteps(transition.Steps))
                {
                    continue;
                }

                foreach (var targetState in GetTransitionTargetStates(transition.Steps))
                {
                    if (!_states.ContainsKey(targetState))
                    {
                        throw new InvalidOperationException(
                            $"State '{definition.State}' transitions to '{targetState}', but it is not configured.");
                    }
                }

                if (transition.HasTargetState && !_states.ContainsKey(transition.TargetState!))
                {
                    throw new InvalidOperationException(
                        $"State '{definition.State}' transitions to '{transition.TargetState}', but it is not configured.");
                }
            }

            foreach (var transition in definition.ImmediateTransitions)
            {
                if (!transition.HasTargetState)
                {
                    throw new InvalidOperationException(
                        $"State '{definition.State}' has an immediate transition with no target state configured.");
                }

                if (!_states.ContainsKey(transition.TargetState!))
                {
                    throw new InvalidOperationException(
                        $"State '{definition.State}' transitions immediately to '{transition.TargetState}', but it is not configured.");
                }
            }
        }

        if (_hasInitialState)
        {
            ResolveInitialLeaf(_initialState!);
        }

        // Run static analysis (unless skipped)
        if (!skipAnalysis)
        {
            var analysis = StateMachineAnalyzer<TState, TTrigger, TData, TCommand>.Analyze(_states, _initialState!);
            
            // Errors are treated as validation failures
            if (!analysis.IsValid)
            {
                throw new InvalidOperationException(
                    "State machine validation detected errors:\n" + 
                    string.Join("\n", analysis.Errors.Select(e => $"  - {e}")));
            }

            // Warnings are logged/reported (we could use ILogger here if needed)
            if (analysis.Warnings.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("State machine analysis detected warnings:");
                foreach (var warning in analysis.Warnings)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {warning}");
                }
            }
        }
    }

    private void ValidateNoCycles(TState state)
    {
        var visited = new HashSet<TState>();
        var current = state;
        while (true)
        {
            if (!_states.TryGetValue(current, out var definition))
            {
                return;
            }

            if (!visited.Add(current))
            {
                throw new InvalidOperationException($"Cycle detected in state hierarchy at '{current}'.");
            }

            if (!definition.HasParentState)
            {
                return;
            }

            current = definition.ParentState;
        }
    }

    private bool TryGetTransitionsInHierarchy(
        TState state,
        TTrigger trigger,
        out List<TransitionDefinition> transitions)
    {
        var current = state;
        while (true)
        {
            if (!_states.TryGetValue(current, out var definition))
            {
                transitions = [];
                return false;
            }

            if (definition.TryGetTransitions(trigger, out transitions))
            {
                return true;
            }

            if (!definition.HasParentState)
            {
                transitions = [];
                return false;
            }

            current = definition.ParentState;
        }
    }

    private TState ResolveInitialLeaf(TState state)
    {
        var current = state;
        var visited = new HashSet<TState>();
        while (true)
        {
            if (!_states.TryGetValue(current, out var definition))
            {
                return current;
            }

            if (!definition.HasChildren)
            {
                return current;
            }

            if (!definition.HasInitialSubState)
            {
                throw new InvalidOperationException(
                    $"State '{current}' has child states but no initial sub-state configured. Call StartsWith.");
            }

            if (!visited.Add(current))
            {
                throw new InvalidOperationException($"Cycle detected in state hierarchy at '{current}'.");
            }

            current = definition.InitialSubState;
        }
    }

    private void AppendExitCommands(
        List<TCommand> commands,
        TState currentState,
        TState targetState,
        TData currentData)
    {
        if (TryFindLowestCommonAncestor(currentState, targetState, out var lca))
        {
            foreach (var state in GetStatesUntil(currentState, lca))
            {
                AppendCommands(commands, _states[state].ExitActions, state, currentData);
            }

            return;
        }

        foreach (var state in GetStatesToRoot(currentState))
        {
            AppendCommands(commands, _states[state].ExitActions, state, currentData);
        }
    }

    private void AppendEntryCommands(
        List<TCommand> commands,
        TState currentState,
        TState targetState,
        TData updatedData)
    {
        if (!_states.ContainsKey(targetState))
        {
            return;
        }

        List<TState> entryStates;
        if (TryFindLowestCommonAncestor(currentState, targetState, out var lca))
        {
            entryStates = GetStatesUntil(targetState, lca);
        }
        else
        {
            entryStates = GetStatesToRoot(targetState);
        }

        entryStates.Reverse();
        foreach (var state in entryStates)
        {
            AppendCommands(commands, _states[state].EntryActions, state, updatedData);
        }
    }

    private void AppendInitialEntryCommands(
        List<TCommand> commands,
        TState targetState,
        TData data)
    {
        if (!_states.ContainsKey(targetState))
        {
            return;
        }

        var entryStates = GetStatesToRoot(targetState);
        entryStates.Reverse();
        foreach (var state in entryStates)
        {
            AppendCommands(commands, _states[state].EntryActions, state, data);
        }
    }


    private List<TState> GetHierarchyChain(TState state)
    {
        var chain = new List<TState>();
        var current = state;
        while (true)
        {
            if (!_states.TryGetValue(current, out var definition))
            {
                break;
            }

            chain.Add(current);
            if (!definition.HasParentState)
            {
                break;
            }

            current = definition.ParentState;
        }

        return chain;
    }

    private List<TState> GetStatesUntil(TState start, TState stopExclusive)
    {
        var states = new List<TState>();
        var current = start;
        while (!EqualityComparer<TState>.Default.Equals(current, stopExclusive))
        {
            states.Add(current);
            var definition = _states[current];
            if (!definition.HasParentState)
            {
                throw new InvalidOperationException(
                    $"State '{start}' is not within the hierarchy of '{stopExclusive}'.");
            }

            current = definition.ParentState;
        }

        return states;
    }

    private List<TState> GetStatesToRoot(TState start)
    {
        var states = new List<TState>();
        var current = start;
        while (true)
        {
            if (!_states.TryGetValue(current, out var definition))
            {
                break;
            }

            states.Add(current);
            if (!definition.HasParentState)
            {
                break;
            }

            current = definition.ParentState;
        }

        return states;
    }

    private static bool HasTransitionTargetSteps(List<TransitionStep> steps)
    {
        return GetTransitionTargetStates(steps).Count > 0;
    }

    private static HashSet<TState> GetTransitionTargetStates(List<TransitionStep> steps)
    {
        var targets = new HashSet<TState>();
        CollectTransitionTargetStates(steps, targets);
        return targets;
    }

    private static void CollectTransitionTargetStates(List<TransitionStep> steps, HashSet<TState> targets)
    {
        foreach (var step in steps)
        {
            switch (step.Kind)
            {
                case TransitionStepKind.Transition:
                    targets.Add(step.TargetState!);
                    break;
                case TransitionStepKind.Conditional:
                    CollectTransitionTargetStates(step.ConditionalTrueSteps!, targets);
                    CollectTransitionTargetStates(step.ConditionalFalseSteps!, targets);
                    break;
                case TransitionStepKind.ConditionalChain:
                    foreach (var branch in step.ConditionalBranches!)
                    {
                        CollectTransitionTargetStates(branch.Steps, targets);
                    }
                    break;
            }
        }
    }

    private bool TryFindLowestCommonAncestor(TState currentState, TState targetState, out TState lca)
    {
        var currentChain = GetHierarchyChain(currentState);
        var currentSet = new HashSet<TState>(currentChain);
        foreach (var state in GetHierarchyChain(targetState))
        {
            if (currentSet.Contains(state))
            {
                lca = state;
                return true;
            }
        }

        lca = default!;
        return false;
    }

    private StateDefinition GetOrCreateState(TState state)
    {
        if (!_states.TryGetValue(state, out var definition))
        {
            definition = new StateDefinition(state);
            _states.Add(state, definition);
        }

        return definition;
    }

    private static object GetTriggerKey(TTrigger trigger)
    {
        var triggerType = trigger.GetType();
        return triggerType != typeof(TTrigger) ? triggerType : trigger;
    }

    private static void AppendCommands(
        List<TCommand> commands,
        List<Func<TState, TData, IEnumerable<TCommand>>> actions,
        TState state,
        TData data)
    {
        foreach (var action in actions)
        {
            foreach (var command in action(state, data) ?? [])
            {
                commands.Add(command);
            }
        }
    }

    private (TState State, TData Data) ApplyImmediateTransitions(
        List<TCommand> commands,
        TState currentState,
        TData currentData)
    {
        if (!_states.TryGetValue(currentState, out var definition)
            || definition.ImmediateTransitions.Count == 0)
        {
            return (currentState, currentData);
        }

        var iterationsRemaining = _states.Count + 1;
        var state = currentState;
        var data = currentData;
        var trigger = default(TTrigger)!;

        while (iterationsRemaining-- > 0)
        {
            if (!_states.TryGetValue(state, out definition)
                || definition.ImmediateTransitions.Count == 0)
            {
                return (state, data);
            }

            TransitionDefinition? matched = null;
            foreach (var transition in definition.ImmediateTransitions)
            {
                if (transition.Guard != null && !transition.Guard(state, data, trigger))
                {
                    continue;
                }

                matched = transition;
                break;
            }

            if (matched is null)
            {
                return (state, data);
            }

            if (!matched.HasTargetState)
            {
                throw new InvalidOperationException(
                    $"Immediate transition from '{state}' has no target state configured.");
            }

            var targetState = ResolveInitialLeaf(matched.TargetState!);
            var updatedData = ApplyTransitionSteps(commands, matched.Steps, state, data, trigger, out _, out _);
            var isStateChange = !EqualityComparer<TState>.Default.Equals(state, targetState);

            if (isStateChange)
            {
                AppendExitCommands(commands, state, targetState, updatedData);
                AppendEntryCommands(commands, state, targetState, updatedData);
            }

            state = targetState;
            data = updatedData;
        }

        throw new InvalidOperationException(
            $"Immediate transition loop detected starting at '{currentState}'.");
    }

    private static TData ApplyTransitionSteps(
        List<TCommand> commands,
        List<TransitionStep> steps,
        TState currentState,
        TData currentData,
        TTrigger trigger,
        out bool hasTargetState,
        out TState targetState)
    {
        hasTargetState = false;
        targetState = default!;
        var updatedData = currentData;
        foreach (var step in steps)
        {
            switch (step.Kind)
            {
                case TransitionStepKind.ModifyData:
                    updatedData = step.DataUpdater!(currentState, updatedData, trigger);
                    break;
                case TransitionStepKind.Execute:
                    foreach (var command in step.Executor!(currentState, updatedData, trigger) ?? [])
                    {
                        commands.Add(command);
                    }
                    break;
                case TransitionStepKind.Transition:
                    hasTargetState = true;
                    targetState = step.TargetState!;
                    break;
                case TransitionStepKind.Conditional:
                    var branch = step.Predicate!(currentState, updatedData, trigger)
                        ? step.ConditionalTrueSteps!
                        : step.ConditionalFalseSteps!;
                    updatedData = ApplyTransitionSteps(
                        commands,
                        branch,
                        currentState,
                        updatedData,
                        trigger,
                        out var conditionalHasTargetState,
                        out var conditionalTargetState);
                    if (conditionalHasTargetState && !hasTargetState)
                    {
                        hasTargetState = true;
                        targetState = conditionalTargetState;
                    }
                    break;
                case TransitionStepKind.ConditionalChain:
                    var matchedBranch = step.ConditionalBranches!.FirstOrDefault(b => 
                        b.Predicate(currentState, updatedData, trigger));
                    if (matchedBranch.Steps != null)
                    {
                        updatedData = ApplyTransitionSteps(
                            commands,
                            matchedBranch.Steps,
                            currentState,
                            updatedData,
                            trigger,
                            out var chainHasTargetState,
                            out var chainTargetState);
                        if (chainHasTargetState && !hasTargetState)
                        {
                            hasTargetState = true;
                            targetState = chainTargetState;
                        }
                    }
                    break;
            }
        }

        return updatedData;
    }

    internal sealed class StateConfiguration
    {
        private readonly StateMachine<TState, TTrigger, TData, TCommand> _machine;
        private readonly StateDefinition _definition;

        internal StateConfiguration(
            StateMachine<TState, TTrigger, TData, TCommand> machine,
            StateDefinition definition)
        {
            _machine = machine;
            _definition = definition;
        }

        public StateConfiguration OnEntry(Func<TData, TCommand> action)
        {
            _definition.EntryActions.Add((state, data) => [action(data)]);
            return this;
        }

        public StateConfiguration OnEntry(Func<TState, TData, TCommand> action)
        {
            _definition.EntryActions.Add((state, data) => [action(state, data)]);
            return this;
        }

        public StateConfiguration OnEntry(Func<TCommand> action)
        {
            _definition.EntryActions.Add((state, data) => [action()]);
            return this;
        }

        public StateConfiguration OnEntry(Func<TData, IEnumerable<TCommand>> action)
        {
            _definition.EntryActions.Add((state, data) => action(data));
            return this;
        }

        public StateConfiguration OnEntry(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _definition.EntryActions.Add(action);
            return this;
        }

        public StateConfiguration OnEntry(Func<IEnumerable<TCommand>> action)
        {
            _definition.EntryActions.Add((state, data) => action());
            return this;
        }

        public StateConfiguration OnExit(Func<TData, TCommand> action)
        {
            _definition.ExitActions.Add((state, data) => [action(data)]);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, TData, TCommand> action)
        {
            _definition.ExitActions.Add((state, data) => [action(state, data)]);
            return this;
        }

        public StateConfiguration OnExit(Func<TCommand> action)
        {
            _definition.ExitActions.Add((state, data) => [action()]);
            return this;
        }

        public StateConfiguration OnExit(Func<TData, IEnumerable<TCommand>> action)
        {
            _definition.ExitActions.Add((state, data) => action(data));
            return this;
        }

        public StateConfiguration OnExit(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _definition.ExitActions.Add(action);
            return this;
        }

        public StateConfiguration OnExit(Func<IEnumerable<TCommand>> action)
        {
            _definition.ExitActions.Add((state, data) => action());
            return this;
        }

        public ImmediateTransitionConfiguration Immediately()
        {
            var transition = new TransitionDefinition();
            _definition.AddImmediateTransition(transition);
            return new ImmediateTransitionConfiguration(this, transition);
        }

        public TransitionConfiguration On(TTrigger trigger)
        {
            var transition = new TransitionDefinition();
            _definition.AddTransition(GetTriggerKey(trigger), transition);
            return new TransitionConfiguration(this, transition);
        }

        public TransitionConfiguration<TDerivedTrigger> On<TDerivedTrigger>()
            where TDerivedTrigger : TTrigger
        {
            var transition = new TransitionDefinition();
            _definition.AddTransition(typeof(TDerivedTrigger), transition);
            return new TransitionConfiguration<TDerivedTrigger>(this, transition);
        }

        public StateConfiguration For(TState state)
        {
            return _machine.For(state);
        }

        public StateConfiguration SubStateOf(TState parentState)
        {
            _definition.ParentState = parentState;
            _definition.HasParentState = true;
            return this;
        }

        public StateConfiguration StartsWith(TState initialSubState)
        {
            _definition.InitialSubState = initialSubState;
            _definition.HasInitialSubState = true;
            return this;
        }
    }

    internal sealed class ImmediateTransitionConfiguration
    {
        private readonly StateConfiguration _parent;
        private readonly TransitionDefinition _transition;

        internal ImmediateTransitionConfiguration(
            StateConfiguration parent,
            TransitionDefinition transition)
        {
            _parent = parent;
            _transition = transition;
        }

        public ImmediateTransitionConfiguration TransitionTo(TState state)
        {
            _transition.SetTargetState(state);
            return this;
        }

        public ImmediateTransitionConfiguration Guard(Func<TData, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(data);
            return this;
        }

        public ImmediateTransitionConfiguration Guard(Func<TState, TData, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(state, data);
            return this;
        }

        public ImmediateTransitionConfiguration ModifyData(Func<TData, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, data, trigger) => updater(data)));
            return this;
        }

        public ImmediateTransitionConfiguration ModifyData(Func<TState, TData, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, data, trigger) => updater(state, data)));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TData, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action(data)]));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TState, TData, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action(state, data)]));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TData, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action(data)));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action(state, data)));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action()]));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action()));
            return this;
        }

        public StateConfiguration Done()
        {
            return _parent;
        }
    }

    internal sealed class TransitionConfiguration
    {
        private readonly StateConfiguration _parent;
        private readonly TransitionDefinition _transition;

        internal TransitionConfiguration(StateConfiguration parent, TransitionDefinition transition)
        {
            _parent = parent;
            _transition = transition;
        }

        public TransitionConfiguration TransitionTo(TState state)
        {
            _transition.SetTargetState(state);
            return this;
        }

        public TransitionConfiguration Guard(Func<TData, TTrigger, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(data, trigger);
            return this;
        }

        public TransitionConfiguration Guard(Func<TState, TData, TTrigger, bool> guard)
        {
            _transition.Guard = guard;
            return this;
        }

        public TransitionConfiguration Guard(Func<TData, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(data);
            return this;
        }

        public TransitionConfiguration Guard(Func<TState, TData, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(state, data);
            return this;
        }

        public TransitionConfiguration ModifyData(Func<TData, TTrigger, TData> updater)
        {
            _transition.Steps.Add(
                TransitionStep.ForModifyData((state, data, trigger) => updater(data, trigger)));
            return this;
        }

        public TransitionConfiguration ModifyData(Func<TState, TData, TTrigger, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData(updater));
            return this;
        }

        public TransitionConfiguration ModifyData(Func<TData, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, data, trigger) => updater(data)));
            return this;
        }

        public TransitionConfiguration ModifyData(Func<TState, TData, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, data, trigger) => updater(state, data)));
            return this;
        }

        public TransitionConfiguration Execute(Func<TData, TTrigger, TCommand> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, data, trigger) => [action(data, trigger)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TData, TTrigger, TCommand> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, data, trigger) => [action(state, data, trigger)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<TData, TTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, data, trigger) => action(data, trigger)));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TData, TTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute(action));
            return this;
        }

        public TransitionConfiguration Execute(Func<TData, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action(data)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TData, TCommand> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, data, trigger) => [action(state, data)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<TData, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action(data)));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action(state, data)));
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action(trigger)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action(trigger)));
            return this;
        }

        public TransitionConfiguration Execute(Func<TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action()]));
            return this;
        }

        public TransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action()));
            return this;
        }

        public ConditionalTransitionConfiguration If(Func<TData, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(
                this,
                _transition,
                (state, data, trigger) => predicate(data, trigger));
        }

        public ConditionalTransitionConfiguration If(Func<TState, TData, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _transition, predicate);
        }

        public ConditionalTransitionConfiguration If(Func<TData, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _transition, (state, data, trigger) => predicate(data));
        }

        public ConditionalTransitionConfiguration If(Func<TState, TData, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(
                this,
                _transition,
                (state, data, trigger) => predicate(state, data));
        }

        public StateConfiguration Ignore()
        {
            _transition.IsIgnored = true;
            return _parent;
        }

        public StateConfiguration Done()
        {
            return _parent;
        }

        public TransitionConfiguration On(TTrigger trigger)
        {
            return _parent.On(trigger);
        }

        public TransitionConfiguration<TDerivedTrigger> On<TDerivedTrigger>()
            where TDerivedTrigger : TTrigger
        {
            return _parent.On<TDerivedTrigger>();
        }

        public StateConfiguration For(TState state)
        {
            return _parent.For(state);
        }
    }

    internal sealed class TransitionConfiguration<TDerivedTrigger>
        where TDerivedTrigger : TTrigger
    {
        private readonly StateConfiguration _parent;
        private readonly TransitionDefinition _transition;

        internal TransitionConfiguration(StateConfiguration parent, TransitionDefinition transition)
        {
            _parent = parent;
            _transition = transition;
        }

        public TransitionConfiguration<TDerivedTrigger> TransitionTo(TState state)
        {
            _transition.SetTargetState(state);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<TData, TDerivedTrigger, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(data, (TDerivedTrigger)trigger);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(
            Func<TState, TData, TDerivedTrigger, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(state, data, (TDerivedTrigger)trigger);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<TState, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(state);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<TData, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(data);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<TState, TData, bool> guard)
        {
            _transition.Guard = (state, data, trigger) => guard(state, data);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<TData, TDerivedTrigger, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData(
                (state, data, trigger) => updater(data, (TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<TState, TData, TDerivedTrigger, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData(
                (state, data, trigger) => updater(state, data, (TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(Func<TData, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, data, trigger) => updater(data)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(Func<TState, TData, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, data, trigger) => updater(state, data)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TData, TDerivedTrigger, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute(
                (state, data, trigger) => [action(data, (TDerivedTrigger)trigger)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, TDerivedTrigger, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute(
                (state, data, trigger) => [action(state, data, (TDerivedTrigger)trigger)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute(
                (state, data, trigger) => action(data, (TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute(
                (state, data, trigger) => action(state, data, (TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TData, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action(data)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TState, TData, TCommand> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, data, trigger) => [action(state, data)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TData, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action(data)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action(state, data)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TDerivedTrigger, TCommand> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, data, trigger) => [action((TDerivedTrigger)trigger)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, data, trigger) => action((TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => [action()]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, data, trigger) => action()));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<TData, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _transition,
                (state, data, trigger) => predicate(data, (TDerivedTrigger)trigger));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<TState, TData, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _transition,
                (state, data, trigger) => predicate(state, data, (TDerivedTrigger)trigger));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(Func<TData, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _transition,
                (state, data, trigger) => predicate(data));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(Func<TState, TData, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _transition,
                (state, data, trigger) => predicate(state, data));
        }

        public StateConfiguration Ignore()
        {
            _transition.IsIgnored = true;
            return _parent;
        }

        public StateConfiguration Done()
        {
            return _parent;
        }

        public TransitionConfiguration On(TTrigger trigger)
        {
            return _parent.On(trigger);
        }

        public TransitionConfiguration<TNextTrigger> On<TNextTrigger>()
            where TNextTrigger : TTrigger
        {
            return _parent.On<TNextTrigger>();
        }

        public StateConfiguration For(TState state)
        {
            return _parent.For(state);
        }
    }

    internal sealed class ConditionalTransitionConfiguration
    {
        private readonly TransitionConfiguration _parent;
        private readonly TransitionDefinition _transition;
        private readonly List<(Func<TState, TData, TTrigger, bool> Predicate, List<TransitionStep> Steps)> _branches = [];
        private List<TransitionStep>? _currentBranchSteps;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration parent,
            TransitionDefinition transition,
            Func<TState, TData, TTrigger, bool> predicate)
        {
            _parent = parent;
            _transition = transition;
            _branches.Add((predicate, []));
            _currentBranchSteps = _branches[0].Steps;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<TData, TTrigger, TData> updater)
        {
            _currentBranchSteps!.Add(TransitionStep.ForModifyData((state, data, trigger) => updater(data, trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<TState, TData, TTrigger, TData> updater)
        {
            _currentBranchSteps!.Add(TransitionStep.ForModifyData(updater));
            return this;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<TData, TData> updater)
        {
            return ModifyData((state, data, trigger) => updater(data));
        }

        public ConditionalTransitionConfiguration ModifyData(Func<TState, TData, TData> updater)
        {
            return ModifyData((state, data, trigger) => updater(state, data));
        }

        public ConditionalTransitionConfiguration Execute(Func<TData, TTrigger, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(data, trigger)]);
        }

        public ConditionalTransitionConfiguration Execute(Func<TState, TData, TTrigger, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(state, data, trigger)]);
        }

        public ConditionalTransitionConfiguration Execute(
            Func<TData, TTrigger, IEnumerable<TCommand>> action)
        {
            _currentBranchSteps!.Add(TransitionStep.ForExecute((state, data, trigger) => action(data, trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(
            Func<TState, TData, TTrigger, IEnumerable<TCommand>> action)
        {
            _currentBranchSteps!.Add(TransitionStep.ForExecute(action));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<TData, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(data)]);
        }

        public ConditionalTransitionConfiguration Execute(Func<TState, TData, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(state, data)]);
        }

        public ConditionalTransitionConfiguration Execute(Func<TData, IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action(data));
        }

        public ConditionalTransitionConfiguration Execute(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action(state, data));
        }

        public ConditionalTransitionConfiguration Execute(Func<TTrigger, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(trigger)]);
        }

        public ConditionalTransitionConfiguration Execute(Func<TTrigger, IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action(trigger));
        }

        public ConditionalTransitionConfiguration Execute(Func<TCommand> action)
        {
            return Execute((state, data, trigger) => [action()]);
        }

        public ConditionalTransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action());
        }

        public ConditionalTransitionConfiguration TransitionTo(TState state)
        {
            _currentBranchSteps!.Add(TransitionStep.ForTransition(state));
            return this;
        }

        public ConditionalTransitionConfiguration ElseIf(Func<TData, TTrigger, bool> predicate)
        {
            _branches.Add(((state, data, trigger) => predicate(data, trigger), []));
            _currentBranchSteps = _branches[^1].Steps;
            return this;
        }

        public ConditionalTransitionConfiguration ElseIf(Func<TState, TData, TTrigger, bool> predicate)
        {
            _branches.Add((predicate, []));
            _currentBranchSteps = _branches[^1].Steps;
            return this;
        }

        public ConditionalTransitionConfiguration ElseIf(Func<TData, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate(data));
        }

        public ConditionalTransitionConfiguration ElseIf(Func<TState, TData, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate(state, data));
        }

        public ConditionalTransitionConfiguration ElseIf(Func<TState, TTrigger, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate(state, trigger));
        }

        public ConditionalTransitionConfiguration ElseIf(Func<TTrigger, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate(trigger));
        }

        public ConditionalTransitionConfiguration Else()
        {
            _branches.Add(((state, data, trigger) => true, []));
            _currentBranchSteps = _branches[^1].Steps;
            return this;
        }

        public TransitionConfiguration Done()
        {
            _transition.Steps.Add(TransitionStep.ForConditionalChain(_branches));
            return _parent;
        }
    }

    internal sealed class ConditionalTransitionConfiguration<TDerivedTrigger>
        where TDerivedTrigger : TTrigger
    {
        private readonly TransitionConfiguration<TDerivedTrigger> _parent;
        private readonly TransitionDefinition _transition;
        private readonly List<(Func<TState, TData, TTrigger, bool> Predicate, List<TransitionStep> Steps)> _branches = [];
        private List<TransitionStep>? _currentBranchSteps;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration<TDerivedTrigger> parent,
            TransitionDefinition transition,
            Func<TState, TData, TDerivedTrigger, bool> predicate)
        {
            _parent = parent;
            _transition = transition;
            var wrappedPredicate = (Func<TState, TData, TTrigger, bool>)((state, data, trigger) => 
                predicate(state, data, (TDerivedTrigger)trigger));
            _branches.Add((wrappedPredicate, []));
            _currentBranchSteps = _branches[0].Steps;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<TData, TDerivedTrigger, TData> updater)
        {
            _currentBranchSteps!.Add(
                TransitionStep.ForModifyData((state, data, trigger) => updater(data, (TDerivedTrigger)trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<TState, TData, TDerivedTrigger, TData> updater)
        {
            _currentBranchSteps!.Add(TransitionStep.ForModifyData(
                (state, data, trigger) => updater(state, data, (TDerivedTrigger)trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(Func<TData, TData> updater)
        {
            return ModifyData((state, data, trigger) => updater(data));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(Func<TState, TData, TData> updater)
        {
            return ModifyData((state, data, trigger) => updater(state, data));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TData, TDerivedTrigger, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(data, (TDerivedTrigger)trigger)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, TDerivedTrigger, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(state, data, (TDerivedTrigger)trigger)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _currentBranchSteps!.Add(TransitionStep.ForExecute(
                (state, data, trigger) => action(data, (TDerivedTrigger)trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _currentBranchSteps!.Add(TransitionStep.ForExecute(
                (state, data, trigger) => action(state, data, (TDerivedTrigger)trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TData, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(data)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TState, TData, TCommand> action)
        {
            return Execute((state, data, trigger) => [action(state, data)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TData, IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action(data));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action(state, data));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TDerivedTrigger, TCommand> action)
        {
            return Execute((state, data, trigger) => [action((TDerivedTrigger)trigger)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action((TDerivedTrigger)trigger));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TCommand> action)
        {
            return Execute((state, data, trigger) => [action()]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<IEnumerable<TCommand>> action)
        {
            return Execute((state, data, trigger) => action());
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> TransitionTo(TState state)
        {
            _currentBranchSteps!.Add(TransitionStep.ForTransition(state));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(
            Func<TData, TDerivedTrigger, bool> predicate)
        {
            _branches.Add(((state, data, trigger) => predicate(data, (TDerivedTrigger)trigger), []));
            _currentBranchSteps = _branches[^1].Steps;
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(
            Func<TState, TData, TDerivedTrigger, bool> predicate)
        {
            _branches.Add(((state, data, trigger) => 
                predicate(state, data, (TDerivedTrigger)trigger), []));
            _currentBranchSteps = _branches[^1].Steps;
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(Func<TData, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate(data));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(Func<TState, TData, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate(state, data));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(
            Func<TState, TDerivedTrigger, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate(state, (TDerivedTrigger)trigger));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(Func<TDerivedTrigger, bool> predicate)
        {
            return ElseIf((state, data, trigger) => predicate((TDerivedTrigger)trigger));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Else()
        {
            _branches.Add(((state, data, trigger) => true, []));
            _currentBranchSteps = _branches[^1].Steps;
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Done()
        {
            _transition.Steps.Add(TransitionStep.ForConditionalChain(_branches));
            return _parent;
        }
    }

    internal sealed class StateDefinition(TState state)
    {
        private readonly Dictionary<object, List<TransitionDefinition>> _transitions = new();

        public TState State { get; } = state;

        public List<TransitionDefinition> ImmediateTransitions { get; } = [];

        public List<Func<TState, TData, IEnumerable<TCommand>>> EntryActions { get; } = [];

        public List<Func<TState, TData, IEnumerable<TCommand>>> ExitActions { get; } = [];

        public bool HasParentState { get; set; }

        public TState ParentState { get; set; } = default!;

        public bool HasInitialSubState { get; set; }

        public TState InitialSubState { get; set; } = default!;

        public bool HasChildren { get; set; }

        internal IReadOnlyDictionary<object, List<TransitionDefinition>> Transitions => _transitions;

        public void AddImmediateTransition(TransitionDefinition transition)
        {
            ImmediateTransitions.Add(transition);
        }

        public void AddTransition(object triggerKey, TransitionDefinition transition)
        {
            if (!_transitions.TryGetValue(triggerKey, out var list))
            {
                list = [];
                _transitions.Add(triggerKey, list);
            }

            list.Add(transition);
        }

        public bool TryGetTransitions(TTrigger trigger, out List<TransitionDefinition> transitions)
        {
            return _transitions.TryGetValue(GetTriggerKey(trigger), out transitions!);
        }

        public IEnumerable<TransitionDefinition> GetTransitions()
        {
            foreach (var transitions in _transitions.Values)
            {
                foreach (var transition in transitions)
                {
                    yield return transition;
                }
            }
        }
    }

    internal sealed class TransitionDefinition
    {
        public bool HasTargetState { get; private set; }

        public TState? TargetState { get; private set; }

        public bool IsIgnored { get; set; }

        public Func<TState, TData, TTrigger, bool>? Guard { get; set; }

        public List<TransitionStep> Steps { get; } = [];

        public void SetTargetState(TState state)
        {
            TargetState = state;
            HasTargetState = true;
        }
    }

    internal enum TransitionStepKind
    {
        ModifyData,
        Execute,
        Transition,
        Conditional,
        ConditionalChain
    }

    internal sealed class TransitionStep
    {
        private TransitionStep(TransitionStepKind kind)
        {
            Kind = kind;
        }

        public TransitionStepKind Kind { get; }

        public Func<TState, TData, TTrigger, TData>? DataUpdater { get; private init; }

        public Func<TState, TData, TTrigger, IEnumerable<TCommand>>? Executor { get; private init; }

        public Func<TState, TData, TTrigger, bool>? Predicate { get; private init; }

        public TState? TargetState { get; private init; }

        public List<TransitionStep>? ConditionalTrueSteps { get; private init; }

        public List<TransitionStep>? ConditionalFalseSteps { get; private init; }

        public List<(Func<TState, TData, TTrigger, bool> Predicate, List<TransitionStep> Steps)>? ConditionalBranches { get; private init; }

        public static TransitionStep ForModifyData(Func<TState, TData, TTrigger, TData> updater)
        {
            return new TransitionStep(TransitionStepKind.ModifyData) { DataUpdater = updater };
        }

        public static TransitionStep ForExecute(Func<TState, TData, TTrigger, IEnumerable<TCommand>> action)
        {
            return new TransitionStep(TransitionStepKind.Execute) { Executor = action };
        }

        public static TransitionStep ForTransition(TState targetState)
        {
            return new TransitionStep(TransitionStepKind.Transition) { TargetState = targetState };
        }

        public static TransitionStep ForConditional(
            Func<TState, TData, TTrigger, bool> predicate,
            List<TransitionStep> trueSteps,
            List<TransitionStep> falseSteps)
        {
            return new TransitionStep(TransitionStepKind.Conditional)
            {
                Predicate = predicate,
                ConditionalTrueSteps = trueSteps,
                ConditionalFalseSteps = falseSteps
            };
        }

        public static TransitionStep ForConditionalChain(
            List<(Func<TState, TData, TTrigger, bool> Predicate, List<TransitionStep> Steps)> branches)
        {
            return new TransitionStep(TransitionStepKind.ConditionalChain)
            {
                ConditionalBranches = branches
            };
        }
    }

}

public sealed class StateMachine<TState, TTrigger, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
    private readonly StateMachine<TState, TTrigger, NoData, TCommand> _inner = new();

    public static StateMachineBuilder<TState, TTrigger, TCommand> Create()
    {
        return new StateMachineBuilder<TState, TTrigger, TCommand>();
    }

    internal StateMachine<TState, TTrigger, TCommand> StartWith(TState state)
    {
        _inner.StartWith(state);
        return this;
    }

    internal void Validate()
    {
        _inner.Validate();
    }

    public TState CreateState(TState state) => _inner.CreateState(state, new NoData()).State;

    internal StateMachine<TState, TTrigger, TCommand> OnUnhandled(
        Func<TTrigger, TState, IEnumerable<TCommand>> handler)
    {
        _inner.OnUnhandled((trigger, state) => handler(trigger, state));
        return this;
    }

    internal StateConfiguration For(TState state)
    {
        return new StateConfiguration(this, _inner.For(state));
    }

    public (TState NewState, IReadOnlyList<TCommand> Commands) Fire(
        TTrigger trigger,
        TState currentState)
    {
        var (newState, _, commands) = _inner.Fire(trigger, currentState, new NoData());
        return (newState, commands);
    }

    public bool TryFire(
        TTrigger trigger,
        TState currentState,
        out TState newState,
        out IReadOnlyList<TCommand> commands)
    {
        return _inner.TryFire(trigger, currentState, new NoData(), out newState, out _, out commands);
    }

    public TState InitialState => _inner.InitialState;

    public (TState State, IReadOnlyList<TCommand> Commands) Start()
    {
        var (state, _, commands) = _inner.Start(new NoData());
        return (state, commands);
    }


    internal sealed class StateConfiguration
    {
        private readonly StateMachine<TState, TTrigger, TCommand> _machine;
        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.StateConfiguration _inner;

        internal StateConfiguration(
            StateMachine<TState, TTrigger, TCommand> machine,
            StateMachine<TState, TTrigger, NoData, TCommand>.StateConfiguration inner)
        {
            _machine = machine;
            _inner = inner;
        }

        public StateConfiguration OnEntry(Func<TState, TCommand> action)
        {
            _inner.OnEntry((state, data) => action(state));
            return this;
        }

        public StateConfiguration OnEntry(Func<TCommand> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnEntry(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.OnEntry((state, data) => action(state));
            return this;
        }
        public StateConfiguration OnEntry(Func<IEnumerable<TCommand>> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, TCommand> action)
        {
            _inner.OnExit((state, data) => action(state));
            return this;
        }

        public StateConfiguration OnExit(Func<TCommand> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.OnExit((state, data) => action(state));
            return this;
        }

        public StateConfiguration OnExit(Func<IEnumerable<TCommand>> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public ImmediateTransitionConfiguration Immediately()
        {
            return new ImmediateTransitionConfiguration(this, _inner.Immediately());
        }

        public StateConfiguration SubStateOf(TState parentState)
        {
            _inner.SubStateOf(parentState);
            return this;
        }

        public StateConfiguration StartsWith(TState initialSubState)
        {
            _inner.StartsWith(initialSubState);
            return this;
        }

        public TransitionConfiguration On(TTrigger trigger)
        {
            return new TransitionConfiguration(this, _inner.On(trigger));
        }

        public TransitionConfiguration<TDerivedTrigger> On<TDerivedTrigger>()
            where TDerivedTrigger : TTrigger
        {
            return new TransitionConfiguration<TDerivedTrigger>(this, _inner.On<TDerivedTrigger>());
        }

        public StateConfiguration For(TState state)
        {
            return _machine.For(state);
        }
    }

    internal sealed class ImmediateTransitionConfiguration
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.ImmediateTransitionConfiguration _inner;

        internal ImmediateTransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, NoData, TCommand>.ImmediateTransitionConfiguration inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ImmediateTransitionConfiguration TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public ImmediateTransitionConfiguration Guard(Func<TState, bool> guard)
        {
            _inner.Guard((state, data) => guard(state));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TState, TCommand> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ImmediateTransitionConfiguration ModifyData(Func<NoData, NoData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ImmediateTransitionConfiguration ModifyData(Func<TState, NoData> updater)
        {
            _inner.ModifyData((state, data) => updater(state));
            return this;
        }

        public StateConfiguration Done()
        {
            return _parent;
        }
    }

    internal sealed class TransitionConfiguration
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.TransitionConfiguration _inner;

        internal TransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, NoData, TCommand>.TransitionConfiguration inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public TransitionConfiguration TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public TransitionConfiguration Guard(Func<TState, TTrigger, bool> guard)
        {
            _inner.Guard((state, data, trigger) => guard(state, trigger));
            return this;
        }

        public TransitionConfiguration Guard(Func<TState, bool> guard)
        {
            _inner.Guard((state, data) => guard(state));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TTrigger, TCommand> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TCommand> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ConditionalTransitionConfiguration If(Func<TState, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _inner.If((state, data, trigger) => predicate(state, trigger)));
        }

        public ConditionalTransitionConfiguration If(Func<TState, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _inner.If((state, data) => predicate(state)));
        }

        public TransitionConfiguration ModifyData(Func<NoData, NoData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public TransitionConfiguration ModifyData(Func<TState, NoData> updater)
        {
            _inner.ModifyData((state, data) => updater(state));
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public StateConfiguration Ignore()
        {
            _inner.Ignore();
            return _parent;
        }

        public StateConfiguration Done()
        {
            _inner.Done();
            return _parent;
        }

        public TransitionConfiguration On(TTrigger trigger)
        {
            return _parent.On(trigger);
        }

        public TransitionConfiguration<TDerivedTrigger> On<TDerivedTrigger>()
            where TDerivedTrigger : TTrigger
        {
            return _parent.On<TDerivedTrigger>();
        }

        public StateConfiguration For(TState state)
        {
            return _parent.For(state);
        }
    }

    internal sealed class TransitionConfiguration<TDerivedTrigger>
        where TDerivedTrigger : TTrigger
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.TransitionConfiguration<TDerivedTrigger> _inner;

        internal TransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, NoData, TCommand>.TransitionConfiguration<TDerivedTrigger> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public TransitionConfiguration<TDerivedTrigger> TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<TState, TDerivedTrigger, bool> guard)
        {
            _inner.Guard((state, data, trigger) => guard(state, trigger));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<TState, bool> guard)
        {
            _inner.Guard((state, data) => guard(state));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, TCommand> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TState, TCommand> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<TState, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _inner.If((state, data, trigger) => predicate(state, trigger)));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(Func<TState, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _inner.If((state, data) => predicate(state)));
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(Func<NoData, NoData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(Func<TState, NoData> updater)
        {
            _inner.ModifyData((state, data) => updater(state));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TDerivedTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public StateConfiguration Ignore()
        {
            _inner.Ignore();
            return _parent;
        }

        public StateConfiguration Done()
        {
            _inner.Done();
            return _parent;
        }

        public TransitionConfiguration On(TTrigger trigger)
        {
            return _parent.On(trigger);
        }

        public TransitionConfiguration<TNextTrigger> On<TNextTrigger>()
            where TNextTrigger : TTrigger
        {
            return _parent.On<TNextTrigger>();
        }

        public StateConfiguration For(TState state)
        {
            return _parent.For(state);
        }
    }

    internal sealed class ConditionalTransitionConfiguration
    {
        private readonly TransitionConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.ConditionalTransitionConfiguration _inner;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration parent,
            StateMachine<TState, TTrigger, NoData, TCommand>.ConditionalTransitionConfiguration inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<NoData, NoData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<TState, NoData> updater)
        {
            _inner.ModifyData((state, data) => updater(state));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(
            Func<TState, TTrigger, TCommand> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(
            Func<TState, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<TState, TCommand> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<TTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public ConditionalTransitionConfiguration Else()
        {
            _inner.Else();
            return this;
        }

        public TransitionConfiguration Done()
        {
            _inner.Done();
            return _parent;
        }
    }

    internal sealed class ConditionalTransitionConfiguration<TDerivedTrigger>
        where TDerivedTrigger : TTrigger
    {
        private readonly TransitionConfiguration<TDerivedTrigger> _parent;
        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.ConditionalTransitionConfiguration<TDerivedTrigger> _inner;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration<TDerivedTrigger> parent,
            StateMachine<TState, TTrigger, NoData, TCommand>.ConditionalTransitionConfiguration<TDerivedTrigger> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<NoData, NoData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<TState, NoData> updater)
        {
            _inner.ModifyData((state, data) => updater(state));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, TCommand> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TState, TCommand> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data) => action(state));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TDerivedTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Else()
        {
            _inner.Else();
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Done()
        {
            _inner.Done();
            return _parent;
        }
    }
}
