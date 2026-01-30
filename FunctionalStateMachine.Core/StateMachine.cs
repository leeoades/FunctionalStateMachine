using System.Collections.ObjectModel;

namespace FunctionalStateMachine.Core;

public sealed class StateMachine<TState, TTrigger, TData, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
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

    public State<TState, TData> CreateState(TState state, TData data) => new(state, data);

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

    public TState? InitialStateOrDefault() => _hasInitialState ? _initialState : default;

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

        if (definition.SubStateMachine != null)
        {
            if (definition.SubStateMachine.TryFire(trigger, current.Data, out var updatedData, out var subCommands))
            {
                newState = new State<TState, TData>(current.Value, updatedData);
                commands = subCommands;
                return true;
            }
        }

        if (!definition.TryGetTransitions(trigger, out var transitions))
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
            var updatedData = transition.DataUpdater != null
                ? transition.DataUpdater(current, trigger)
                : current.Data;
            var nextState = new State<TState, TData>(targetState, updatedData);

            var commandList = new List<TCommand>();
            var isStateChange = transition.HasTargetState
                && !EqualityComparer<TState>.Default.Equals(current.Value, targetState);

            if (isStateChange)
            {
                AppendCommands(commandList, definition.ExitActions, current);
            }

            AppendTransitionCommands(commandList, transition.Actions, current, trigger);

            if (isStateChange && _states.TryGetValue(targetState, out var nextDefinition))
            {
                AppendCommands(commandList, nextDefinition.EntryActions, nextState);
            }

            newState = nextState;
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

    private StateDefinition GetOrCreateState(TState state)
    {
        if (!_states.TryGetValue(state, out var definition))
        {
            definition = new StateDefinition(state);
            _states.Add(state, definition);
        }

        return definition;
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

    private static void AppendTransitionCommands(
        List<TCommand> commands,
        List<Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>>> actions,
        State<TState, TData> state,
        TTrigger trigger)
    {
        foreach (var action in actions)
        {
            foreach (var command in action(state, trigger) ?? [])
            {
                commands.Add(command);
            }
        }
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
            var transition = new TransitionDefinition(trigger);
            _definition.AddTransition(trigger, transition);
            return new TransitionConfiguration(this, transition);
        }

        public StateConfiguration WithSubStateMachine<TSubState, TSubData>(
            StateMachine<TSubState, TTrigger, TSubData, TCommand> subMachine,
            Func<TData, SubState<TSubState, TSubData>> getSubState,
            Func<TData, SubState<TSubState, TSubData>, TData> setSubState)
            where TSubState : notnull
        {
            _definition.SubStateMachine = new SubStateMachineAdapter<TSubState, TSubData>(
                subMachine,
                getSubState,
                setSubState);
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

        public TransitionConfiguration WithData(Func<State<TState, TData>, TTrigger, TData> updater)
        {
            _transition.DataUpdater = updater;
            return this;
        }

        public TransitionConfiguration WithData(Func<State<TState, TData>, TData> updater)
        {
            _transition.DataUpdater = (state, trigger) => updater(state);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TTrigger, TCommand> action)
        {
            _transition.Actions.Add((state, trigger) => [action(state, trigger)]);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Actions.Add(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TCommand> action)
        {
            _transition.Actions.Add((state, trigger) => [action(state)]);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            _transition.Actions.Add((state, trigger) => action(state));
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, TCommand> action)
        {
            _transition.Actions.Add((state, trigger) => [action(trigger)]);
            return this;
        }

        public TransitionConfiguration Execute(Func<TTrigger, IEnumerable<TCommand>> action)
        {
            _transition.Actions.Add((state, trigger) => action(trigger));
            return this;
        }

        public TransitionConfiguration Execute(Func<TCommand> action)
        {
            _transition.Actions.Add((state, trigger) => [action()]);
            return this;
        }

        public TransitionConfiguration Execute(Func<IEnumerable<TCommand>> action)
        {
            _transition.Actions.Add((state, trigger) => action());
            return this;
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
    }

    internal sealed class StateDefinition
    {
        private readonly Dictionary<TTrigger, List<TransitionDefinition>> _transitions = new();

        public StateDefinition(TState state)
        {
            State = state;
        }

        public TState State { get; }

        public List<Func<State<TState, TData>, IEnumerable<TCommand>>> EntryActions { get; } = [];

        public List<Func<State<TState, TData>, IEnumerable<TCommand>>> ExitActions { get; } = [];

        public ISubStateMachine? SubStateMachine { get; set; }

        public void AddTransition(TTrigger trigger, TransitionDefinition transition)
        {
            if (!_transitions.TryGetValue(trigger, out var list))
            {
                list = [];
                _transitions.Add(trigger, list);
            }

            list.Add(transition);
        }

        public bool TryGetTransitions(TTrigger trigger, out List<TransitionDefinition> transitions)
        {
            return _transitions.TryGetValue(trigger, out transitions!);
        }
    }

    internal sealed class TransitionDefinition
    {
        public TransitionDefinition(TTrigger trigger)
        {
            Trigger = trigger;
        }

        public TTrigger Trigger { get; }

        public bool HasTargetState { get; private set; }

        public TState? TargetState { get; private set; }

        public bool IsIgnored { get; set; }

        public Func<State<TState, TData>, TTrigger, bool>? Guard { get; set; }

        public Func<State<TState, TData>, TTrigger, TData>? DataUpdater { get; set; }

        public List<Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>>> Actions { get; } = [];

        public void SetTargetState(TState state)
        {
            TargetState = state;
            HasTargetState = true;
        }
    }

    internal interface ISubStateMachine
    {
        bool TryFire(TTrigger trigger, TData parentData, out TData newParentData, out IReadOnlyList<TCommand> commands);
    }

    private sealed class SubStateMachineAdapter<TSubState, TSubData> : ISubStateMachine
        where TSubState : notnull
    {
        private readonly StateMachine<TSubState, TTrigger, TSubData, TCommand> _subMachine;
        private readonly Func<TData, SubState<TSubState, TSubData>> _getSubState;
        private readonly Func<TData, SubState<TSubState, TSubData>, TData> _setSubState;

        public SubStateMachineAdapter(
            StateMachine<TSubState, TTrigger, TSubData, TCommand> subMachine,
            Func<TData, SubState<TSubState, TSubData>> getSubState,
            Func<TData, SubState<TSubState, TSubData>, TData> setSubState)
        {
            _subMachine = subMachine;
            _getSubState = getSubState;
            _setSubState = setSubState;
        }

        public bool TryFire(
            TTrigger trigger,
            TData parentData,
            out TData newParentData,
            out IReadOnlyList<TCommand> commands)
        {
            var sub = _getSubState(parentData);
            var current = new State<TSubState, TSubData>(sub.Value, sub.Data);

            if (!_subMachine.TryFire(trigger, current, out var next, out commands))
            {
                newParentData = parentData;
                return false;
            }

            var updatedSub = new SubState<TSubState, TSubData>(next.Value, next.Data);
            newParentData = _setSubState(parentData, updatedSub);
            return true;
        }
    }
}

public sealed class StateMachine<TState, TTrigger, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
    private readonly StateMachine<TState, TTrigger, NoData, TCommand> _inner = new();

    internal StateMachine<TState, TTrigger, TCommand> StartWith(TState state)
    {
        _inner.StartWith(state);
        return this;
    }

    public State<TState, NoData> CreateState(TState state) => _inner.CreateState(state, new NoData());

    internal StateMachine<TState, TTrigger, TCommand> OnUnhandled(Action<TTrigger, State<TState, NoData>> handler)
    {
        _inner.OnUnhandled(handler);
        return this;
    }

    internal StateConfiguration For(TState state)
    {
        return new StateConfiguration(_inner.For(state));
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
        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.StateConfiguration _inner;

        internal StateConfiguration(StateMachine<TState, TTrigger, NoData, TCommand>.StateConfiguration inner)
        {
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

        public TransitionConfiguration On(TTrigger trigger)
        {
            return new TransitionConfiguration(this, _inner.On(trigger));
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

        public TransitionConfiguration WithData(Func<State<TState, NoData>, NoData> updater)
        {
            _inner.WithData(updater);
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
    }
}
