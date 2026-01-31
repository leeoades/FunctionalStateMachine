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
    private Action<TTrigger, State<TState, TData>>? _onUnhandled;
    private bool _hasInitialState;
    private TState? _initialState;

    internal StateMachine<TState, TTrigger, TData, TCommand> StartWith(TState state)
    {
        _hasInitialState = true;
        _initialState = state;
        return this;
    }

    public State<TState, TData> CreateState(TState state, TData data) => new(ResolveInitialLeaf(state), data);

    internal StateConfiguration For(TState state)
    {
        var definition = GetOrCreateState(state);
        return new StateConfiguration(this, definition);
    }

    internal StateMachine<TState, TTrigger, TData, TCommand> OnUnhandled(Action<TTrigger, State<TState, TData>> handler)
    {
        _onUnhandled = handler;
        return this;
    }

    public (State<TState, TData> NewState, IReadOnlyList<TCommand> Commands) Fire(
        TTrigger trigger,
        State<TState, TData> current)
    {
        if (TryFireInternal(trigger, current, out var newState, out var commands, throwOnUnhandled: true))
        {
            return (newState, commands);
        }

        throw new InvalidOperationException("Unhandled trigger.");
    }

    public bool TryFire(
        TTrigger trigger,
        State<TState, TData> current,
        out State<TState, TData> newState,
        out IReadOnlyList<TCommand> commands)
    {
        return TryFireInternal(trigger, current, out newState, out commands, throwOnUnhandled: false);
    }

    public TState? InitialStateOrDefault()
    {
        return _hasInitialState ? ResolveInitialLeaf(_initialState!) : default;
    }

    private bool TryFireInternal(
        TTrigger trigger,
        State<TState, TData> current,
        out State<TState, TData> newState,
        out IReadOnlyList<TCommand> commands,
        bool throwOnUnhandled)
    {
        if (!_states.TryGetValue(current.Value, out var definition))
        {
            throw new InvalidOperationException($"State '{current.Value}' is not configured.");
        }

        if (definition.HasChildren)
        {
            throw new InvalidOperationException(
                $"State '{current.Value}' is a parent state. Use a leaf state instead.");
        }

        if (!TryGetTransitionsInHierarchy(current.Value, trigger, out var transitions))
        {
            return HandleUnhandled(trigger, current, out newState, out commands, throwOnUnhandled);
        }

        foreach (var transition in transitions)
        {
            if (transition.Guard != null && !transition.Guard(current, trigger))
            {
                continue;
            }

            if (transition.IsIgnored)
            {
                newState = current;
                commands = [];
                return true;
            }

            var targetState = transition.HasTargetState ? transition.TargetState! : current.Value;
            targetState = ResolveInitialLeaf(targetState);

            var commandList = new List<TCommand>();
            var isStateChange = transition.HasTargetState
                && !EqualityComparer<TState>.Default.Equals(current.Value, targetState);

            var updatedData = ApplyTransitionSteps(commandList, transition, current, trigger);

            if (isStateChange)
            {
                AppendExitCommands(commandList, current.Value, targetState, updatedData);
                AppendEntryCommands(commandList, current.Value, targetState, updatedData);
            }

            newState = new State<TState, TData>(targetState, updatedData);
            commands = commandList.Count == 0 ? Array.Empty<TCommand>() : new ReadOnlyCollection<TCommand>(commandList);
            return true;
        }

        return HandleUnhandled(trigger, current, out newState, out commands, throwOnUnhandled);
    }

    private bool HandleUnhandled(
        TTrigger trigger,
        State<TState, TData> current,
        out State<TState, TData> newState,
        out IReadOnlyList<TCommand> commands,
        bool throwOnUnhandled)
    {
        if (_onUnhandled != null)
        {
            _onUnhandled(trigger, current);
            newState = current;
            commands = [];
            return true;
        }

        if (throwOnUnhandled)
        {
            throw new InvalidOperationException($"Unhandled trigger '{trigger}' in state '{current.Value}'.");
        }

        newState = current;
        commands = [];
        return false;
    }

    internal void Validate()
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
                if (!transition.HasTargetState)
                {
                    continue;
                }

                if (!_states.ContainsKey(transition.TargetState!))
                {
                    throw new InvalidOperationException(
                        $"State '{definition.State}' transitions to '{transition.TargetState}', but it is not configured.");
                }
            }
        }

        if (_hasInitialState)
        {
            ResolveInitialLeaf(_initialState!);
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
                AppendCommands(commands, _states[state].ExitActions, new State<TState, TData>(state, currentData));
            }

            return;
        }

        foreach (var state in GetStatesToRoot(currentState))
        {
            AppendCommands(commands, _states[state].ExitActions, new State<TState, TData>(state, currentData));
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
            AppendCommands(commands, _states[state].EntryActions, new State<TState, TData>(state, updatedData));
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
        List<Func<State<TState, TData>, IEnumerable<TCommand>>> actions,
        State<TState, TData> state)
    {
        foreach (var action in actions)
        {
            foreach (var command in action(state) ?? [])
            {
                commands.Add(command);
            }
        }
    }

    private static TData ApplyTransitionSteps(
        List<TCommand> commands,
        TransitionDefinition transition,
        State<TState, TData> current,
        TTrigger trigger)
    {
        return ApplyTransitionSteps(commands, transition.Steps, current, trigger);
    }

    private static TData ApplyTransitionSteps(
        List<TCommand> commands,
        List<TransitionStep> steps,
        State<TState, TData> current,
        TTrigger trigger)
    {
        var updatedData = current.Data;
        foreach (var step in steps)
        {
            var stepState = new State<TState, TData>(current.Value, updatedData);
            switch (step.Kind)
            {
                case TransitionStepKind.ModifyData:
                    updatedData = step.DataUpdater!(stepState, trigger);
                    break;
                case TransitionStepKind.Execute:
                    foreach (var command in step.Executor!(stepState, trigger) ?? [])
                    {
                        commands.Add(command);
                    }
                    break;
                case TransitionStepKind.Conditional:
                    var branch = step.Predicate!(stepState, trigger)
                        ? step.ConditionalTrueSteps!
                        : step.ConditionalFalseSteps!;
                    updatedData = ApplyTransitionSteps(commands, branch, stepState, trigger);
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

        public StateConfiguration OnEntry(Func<State<TState, TData>, TCommand> action)
        {
            _definition.EntryActions.Add(state => [action(state)]);
            return this;
        }

        public StateConfiguration OnEntry(Func<TState, TCommand> action)
        {
            _definition.EntryActions.Add(state => [action(state.Value)]);
            return this;
        }

        public StateConfiguration OnEntry(Func<TCommand> action)
        {
            _definition.EntryActions.Add(state => [action()]);
            return this;
        }

        public StateConfiguration OnEntry(Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            _definition.EntryActions.Add(action);
            return this;
        }

        public StateConfiguration OnEntry(Func<TState, IEnumerable<TCommand>> action)
        {
            _definition.EntryActions.Add(state => action(state.Value));
            return this;
        }

        public StateConfiguration OnEntry(Func<IEnumerable<TCommand>> action)
        {
            _definition.EntryActions.Add(state => action());
            return this;
        }

        public StateConfiguration OnExit(Func<State<TState, TData>, TCommand> action)
        {
            _definition.ExitActions.Add(state => [action(state)]);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, TCommand> action)
        {
            _definition.ExitActions.Add(state => [action(state.Value)]);
            return this;
        }

        public StateConfiguration OnExit(Func<TCommand> action)
        {
            _definition.ExitActions.Add(state => [action()]);
            return this;
        }

        public StateConfiguration OnExit(Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            _definition.ExitActions.Add(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, IEnumerable<TCommand>> action)
        {
            _definition.ExitActions.Add(state => action(state.Value));
            return this;
        }

        public StateConfiguration OnExit(Func<IEnumerable<TCommand>> action)
        {
            _definition.ExitActions.Add(state => action());
            return this;
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

        public TransitionConfiguration Guard(Func<State<TState, TData>, TTrigger, bool> guard)
        {
            _transition.Guard = guard;
            return this;
        }

        public TransitionConfiguration Guard(Func<State<TState, TData>, bool> guard)
        {
            _transition.Guard = (state, trigger) => guard(state);
            return this;
        }

        public TransitionConfiguration ModifyData(Func<State<TState, TData>, TTrigger, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData(updater));
            return this;
        }

        public TransitionConfiguration ModifyData(Func<State<TState, TData>, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, trigger) => updater(state)));
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TTrigger, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => [action(state, trigger)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute(action));
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => [action(state)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => action(state)));
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => [action(trigger)]));
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => action(trigger)));
            return this;
        }

        public TransitionConfiguration Execute(Func<TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => [action()]));
            return this;
        }

        public TransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => action()));
            return this;
        }

        public ConditionalTransitionConfiguration If(Func<State<TState, TData>, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _transition, predicate);
        }

        public ConditionalTransitionConfiguration If(Func<State<TState, TData>, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _transition, (state, trigger) => predicate(state));
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

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<State<TState, TData>, TDerivedTrigger, bool> guard)
        {
            _transition.Guard = (state, trigger) => guard(state, (TDerivedTrigger)trigger);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<State<TState, TData>, bool> guard)
        {
            _transition.Guard = (state, trigger) => guard(state);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<State<TState, TData>, TDerivedTrigger, TData> updater)
        {
            _transition.Steps.Add(
                TransitionStep.ForModifyData((state, trigger) => updater(state, (TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(Func<State<TState, TData>, TData> updater)
        {
            _transition.Steps.Add(TransitionStep.ForModifyData((state, trigger) => updater(state)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, TData>, TDerivedTrigger, TCommand> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, trigger) => [action(state, (TDerivedTrigger)trigger)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, TData>, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, trigger) => action(state, (TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<State<TState, TData>, TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => [action(state)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => action(state)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TDerivedTrigger, TCommand> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, trigger) => [action((TDerivedTrigger)trigger)]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(
                TransitionStep.ForExecute((state, trigger) => action((TDerivedTrigger)trigger)));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<TCommand> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => [action()]));
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<IEnumerable<TCommand>> action)
        {
            _transition.Steps.Add(TransitionStep.ForExecute((state, trigger) => action()));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<State<TState, TData>, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(this, _transition, predicate);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(Func<State<TState, TData>, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _transition,
                (state, trigger) => predicate(state));
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
        private readonly Func<State<TState, TData>, TTrigger, bool> _predicate;
        private readonly List<TransitionStep> _trueSteps = [];
        private readonly List<TransitionStep> _falseSteps = [];
        private bool _useElse;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration parent,
            TransitionDefinition transition,
            Func<State<TState, TData>, TTrigger, bool> predicate)
        {
            _parent = parent;
            _transition = transition;
            _predicate = predicate;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<State<TState, TData>, TTrigger, TData> updater)
        {
            CurrentSteps().Add(TransitionStep.ForModifyData(updater));
            return this;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<State<TState, TData>, TData> updater)
        {
            return ModifyData((state, trigger) => updater(state));
        }

        public ConditionalTransitionConfiguration Execute(
            Func<State<TState, TData>, TTrigger, TCommand> action)
        {
            return Execute((state, trigger) => [action(state, trigger)]);
        }

        public ConditionalTransitionConfiguration Execute(
            Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>> action)
        {
            CurrentSteps().Add(TransitionStep.ForExecute(action));
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<State<TState, TData>, TCommand> action)
        {
            return Execute((state, trigger) => [action(state)]);
        }

        public ConditionalTransitionConfiguration Execute(Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            return Execute((state, trigger) => action(state));
        }

        public ConditionalTransitionConfiguration Execute(Func<TTrigger, TCommand> action)
        {
            return Execute((state, trigger) => [action(trigger)]);
        }

        public ConditionalTransitionConfiguration Execute(Func<TTrigger, IEnumerable<TCommand>> action)
        {
            return Execute((state, trigger) => action(trigger));
        }

        public ConditionalTransitionConfiguration Execute(Func<TCommand> action)
        {
            return Execute((state, trigger) => [action()]);
        }

        public ConditionalTransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            return Execute((state, trigger) => action());
        }

        public ConditionalTransitionConfiguration Else()
        {
            _useElse = true;
            return this;
        }

        public TransitionConfiguration Done()
        {
            _transition.Steps.Add(TransitionStep.ForConditional(_predicate, _trueSteps, _falseSteps));
            return _parent;
        }

        private List<TransitionStep> CurrentSteps()
        {
            return _useElse ? _falseSteps : _trueSteps;
        }
    }

    internal sealed class ConditionalTransitionConfiguration<TDerivedTrigger>
        where TDerivedTrigger : TTrigger
    {
        private readonly TransitionConfiguration<TDerivedTrigger> _parent;
        private readonly TransitionDefinition _transition;
        private readonly Func<State<TState, TData>, TTrigger, bool> _predicate;
        private readonly List<TransitionStep> _trueSteps = [];
        private readonly List<TransitionStep> _falseSteps = [];
        private bool _useElse;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration<TDerivedTrigger> parent,
            TransitionDefinition transition,
            Func<State<TState, TData>, TDerivedTrigger, bool> predicate)
        {
            _parent = parent;
            _transition = transition;
            _predicate = (state, trigger) => predicate(state, (TDerivedTrigger)trigger);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<State<TState, TData>, TDerivedTrigger, TData> updater)
        {
            CurrentSteps().Add(
                TransitionStep.ForModifyData((state, trigger) => updater(state, (TDerivedTrigger)trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<State<TState, TData>, TData> updater)
        {
            return ModifyData((state, trigger) => updater(state));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, TData>, TDerivedTrigger, TCommand> action)
        {
            return Execute((state, trigger) => [action(state, trigger)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, TData>, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            CurrentSteps().Add(
                TransitionStep.ForExecute((state, trigger) => action(state, (TDerivedTrigger)trigger)));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, TData>, TCommand> action)
        {
            return Execute((state, trigger) => [action(state)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            return Execute((state, trigger) => action(state));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TDerivedTrigger, TCommand> action)
        {
            return Execute((state, trigger) => [action(trigger)]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            return Execute((state, trigger) => action(trigger));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<TCommand> action)
        {
            return Execute((state, trigger) => [action()]);
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<IEnumerable<TCommand>> action)
        {
            return Execute((state, trigger) => action());
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Else()
        {
            _useElse = true;
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Done()
        {
            _transition.Steps.Add(TransitionStep.ForConditional(_predicate, _trueSteps, _falseSteps));
            return _parent;
        }

        private List<TransitionStep> CurrentSteps()
        {
            return _useElse ? _falseSteps : _trueSteps;
        }
    }

    internal sealed class StateDefinition
    {
        private readonly Dictionary<object, List<TransitionDefinition>> _transitions = new();

        public StateDefinition(TState state)
        {
            State = state;
        }

        public TState State { get; }

        public List<Func<State<TState, TData>, IEnumerable<TCommand>>> EntryActions { get; } = [];

        public List<Func<State<TState, TData>, IEnumerable<TCommand>>> ExitActions { get; } = [];

        public bool HasParentState { get; set; }

        public TState ParentState { get; set; } = default!;

        public bool HasInitialSubState { get; set; }

        public TState InitialSubState { get; set; } = default!;

        public bool HasChildren { get; set; }

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

        public Func<State<TState, TData>, TTrigger, bool>? Guard { get; set; }

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
        Conditional
    }

    internal sealed class TransitionStep
    {
        private TransitionStep(TransitionStepKind kind)
        {
            Kind = kind;
        }

        public TransitionStepKind Kind { get; }

        public Func<State<TState, TData>, TTrigger, TData>? DataUpdater { get; private init; }

        public Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>>? Executor { get; private init; }

        public Func<State<TState, TData>, TTrigger, bool>? Predicate { get; private init; }

        public List<TransitionStep>? ConditionalTrueSteps { get; private init; }

        public List<TransitionStep>? ConditionalFalseSteps { get; private init; }

        public static TransitionStep ForModifyData(Func<State<TState, TData>, TTrigger, TData> updater)
        {
            return new TransitionStep(TransitionStepKind.ModifyData) { DataUpdater = updater };
        }

        public static TransitionStep ForExecute(Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>> action)
        {
            return new TransitionStep(TransitionStepKind.Execute) { Executor = action };
        }

        public static TransitionStep ForConditional(
            Func<State<TState, TData>, TTrigger, bool> predicate,
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

    public State<TState, NoData> CreateState(TState state) => _inner.CreateState(state, new NoData());

    internal StateMachine<TState, TTrigger, TCommand> OnUnhandled(Action<TTrigger, State<TState, NoData>> handler)
    {
        _inner.OnUnhandled(handler);
        return this;
    }

    internal StateConfiguration For(TState state)
    {
        return new StateConfiguration(this, _inner.For(state));
    }

    public (State<TState, NoData> NewState, IReadOnlyList<TCommand> Commands) Fire(
        TTrigger trigger,
        TState currentState)
    {
        return _inner.Fire(trigger, new State<TState, NoData>(currentState, new NoData()));
    }

    public (State<TState, NoData> NewState, IReadOnlyList<TCommand> Commands) Fire(
        TTrigger trigger,
        State<TState, NoData> current)
    {
        return _inner.Fire(trigger, current);
    }

    public bool TryFire(
        TTrigger trigger,
        State<TState, NoData> current,
        out State<TState, NoData> newState,
        out IReadOnlyList<TCommand> commands)
    {
        return _inner.TryFire(trigger, current, out newState, out commands);
    }

    public TState? InitialStateOrDefault() => _inner.InitialStateOrDefault();

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

        public StateConfiguration OnEntry(Func<State<TState, NoData>, TCommand> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnEntry(Func<TState, TCommand> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnEntry(Func<TCommand> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnEntry(Func<State<TState, NoData>, IEnumerable<TCommand>> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnEntry(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.OnEntry(action);
            return this;
        }
        public StateConfiguration OnEntry(Func<IEnumerable<TCommand>> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnExit(Func<State<TState, NoData>, TCommand> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, TCommand> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TCommand> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public StateConfiguration OnExit(Func<State<TState, NoData>, IEnumerable<TCommand>> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public StateConfiguration OnExit(Func<IEnumerable<TCommand>> action)
        {
            _inner.OnExit(action);
            return this;
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

        public TransitionConfiguration Guard(Func<State<TState, NoData>, TTrigger, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration Guard(Func<State<TState, NoData>, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, NoData>, TTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, NoData>, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, NoData>, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, NoData>, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration If(Func<State<TState, NoData>, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _inner.If(predicate));
        }

        public ConditionalTransitionConfiguration If(Func<State<TState, NoData>, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _inner.If(predicate));
        }

        public TransitionConfiguration ModifyData(Func<State<TState, NoData>, NoData> updater)
        {
            _inner.ModifyData(updater);
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

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<State<TState, NoData>, TDerivedTrigger, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(Func<State<TState, NoData>, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, NoData>, TDerivedTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, NoData>, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<State<TState, NoData>, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(Func<State<TState, NoData>, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<State<TState, NoData>, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(this, _inner.If(predicate));
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(Func<State<TState, NoData>, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(this, _inner.If(predicate));
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(Func<State<TState, NoData>, NoData> updater)
        {
            _inner.ModifyData(updater);
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

        public ConditionalTransitionConfiguration ModifyData(Func<State<TState, NoData>, NoData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(
            Func<State<TState, NoData>, TTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(
            Func<State<TState, NoData>, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<State<TState, NoData>, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(Func<State<TState, NoData>, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
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
            Func<State<TState, NoData>, NoData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, NoData>, TDerivedTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, NoData>, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(Func<State<TState, NoData>, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<State<TState, NoData>, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
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
