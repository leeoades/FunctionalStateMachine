namespace FunctionalStateMachine.Core;

public sealed class StateMachineBuilder<TState, TTrigger, TData, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
    private readonly StateMachine<TState, TTrigger, TData, TCommand> _machine = new();

    public StateMachineBuilder<TState, TTrigger, TData, TCommand> StartWith(TState state)
    {
        _machine.StartWith(state);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger, TData, TCommand> OnUnhandled(
        Action<TTrigger, State<TState, TData>> handler)
    {
        _machine.OnUnhandled(handler);
        return this;
    }

    public StateConfiguration For(TState state)
    {
        return new StateConfiguration(this, _machine.For(state));
    }

    public StateMachine<TState, TTrigger, TData, TCommand> Build() => _machine;

    public sealed class StateConfiguration
    {
        private readonly StateMachineBuilder<TState, TTrigger, TData, TCommand> _builder;
        private readonly StateMachine<TState, TTrigger, TData, TCommand>.StateConfiguration _inner;

        internal StateConfiguration(
            StateMachineBuilder<TState, TTrigger, TData, TCommand> builder,
            StateMachine<TState, TTrigger, TData, TCommand>.StateConfiguration inner)
        {
            _builder = builder;
            _inner = inner;
        }

        public StateConfiguration OnEntry(Func<State<TState, TData>, TCommand> action)
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

        public StateConfiguration OnEntry(Func<State<TState, TData>, IEnumerable<TCommand>> action)
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

        public StateConfiguration OnExit(Func<State<TState, TData>, TCommand> action)
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

        public StateConfiguration OnExit(Func<State<TState, TData>, IEnumerable<TCommand>> action)
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

        public StateConfiguration For(TState state)
        {
            return _builder.For(state);
        }

        public StateMachine<TState, TTrigger, TData, TCommand> Build()
        {
            return _builder.Build();
        }

        public StateConfiguration WithSubStateMachine<TSubState, TSubData>(
            StateMachine<TSubState, TTrigger, TSubData, TCommand> subMachine,
            Func<TData, SubState<TSubState, TSubData>> getSubState,
            Func<TData, SubState<TSubState, TSubData>, TData> setSubState)
            where TSubState : notnull
        {
            _inner.WithSubStateMachine(subMachine, getSubState, setSubState);
            return this;
        }
    }

    public sealed class TransitionConfiguration
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, TData, TCommand>.TransitionConfiguration _inner;

        internal TransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, TData, TCommand>.TransitionConfiguration inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public TransitionConfiguration TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public TransitionConfiguration Guard(Func<State<TState, TData>, TTrigger, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration Guard(Func<State<TState, TData>, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration WithData(Func<State<TState, TData>, TTrigger, TData> updater)
        {
            _inner.WithData(updater);
            return this;
        }

        public TransitionConfiguration WithData(Func<State<TState, TData>, TData> updater)
        {
            _inner.WithData(updater);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TTrigger, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, TCommand> action)
        {
            _inner.Execute(action);
            return this;
        }

        public TransitionConfiguration Execute(Func<State<TState, TData>, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
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

        public StateConfiguration For(TState state)
        {
            return _parent.For(state);
        }

        public StateMachine<TState, TTrigger, TData, TCommand> Build()
        {
            return _parent.Build();
        }
    }
}

public sealed class StateMachineBuilder<TState, TTrigger, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
    private readonly StateMachine<TState, TTrigger, TCommand> _machine = new();

    public StateMachineBuilder<TState, TTrigger, TCommand> StartWith(TState state)
    {
        _machine.StartWith(state);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger, TCommand> OnUnhandled(
        Action<TTrigger, State<TState, NoData>> handler)
    {
        _machine.OnUnhandled(handler);
        return this;
    }

    public StateConfiguration For(TState state)
    {
        return new StateConfiguration(this, _machine.For(state));
    }

    public StateMachine<TState, TTrigger, TCommand> Build() => _machine;

    public sealed class StateConfiguration
    {
        private readonly StateMachineBuilder<TState, TTrigger, TCommand> _builder;
        private readonly StateMachine<TState, TTrigger, TCommand>.StateConfiguration _inner;

        internal StateConfiguration(
            StateMachineBuilder<TState, TTrigger, TCommand> builder,
            StateMachine<TState, TTrigger, TCommand>.StateConfiguration inner)
        {
            _builder = builder;
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

        public StateConfiguration For(TState state)
        {
            return _builder.For(state);
        }

        public StateMachine<TState, TTrigger, TCommand> Build()
        {
            return _builder.Build();
        }
    }

    public sealed class TransitionConfiguration
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, TCommand>.TransitionConfiguration _inner;

        internal TransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, TCommand>.TransitionConfiguration inner)
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

        public TransitionConfiguration On(TTrigger trigger)
        {
            return _parent.On(trigger);
        }

        public StateConfiguration For(TState state)
        {
            return _parent.For(state);
        }

        public StateMachine<TState, TTrigger, TCommand> Build()
        {
            return _parent.Build();
        }
    }
}
