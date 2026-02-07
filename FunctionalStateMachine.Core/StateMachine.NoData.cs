namespace FunctionalStateMachine.Core;

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

    internal StateMachine<TState, TTrigger, TCommand> OnUnhandled(
        Func<TTrigger, TState, IEnumerable<TCommand>> handler)
    {
        _inner.OnUnhandled(handler);
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

    public TState InitialState => _inner.InitialState;


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

        public TransitionConfiguration On(TTrigger trigger)
        {
            return new TransitionConfiguration(this, _inner.On(trigger));
        }

        public TransitionConfiguration<TDerivedTrigger> On<TDerivedTrigger>()
            where TDerivedTrigger : TTrigger
        {
            return new TransitionConfiguration<TDerivedTrigger>(this, _inner.On<TDerivedTrigger>());
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

        public TransitionConfiguration Execute(Func<TState, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration If(Func<TState, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this,
                _inner.If((state, data, trigger) => predicate(state, trigger)));
        }

        public StateConfiguration Done()
        {
            _inner.Done();
            return _parent;
        }
    }

    internal sealed class TransitionConfiguration<TDerivedTrigger>
        where TDerivedTrigger : TTrigger
    {
        private readonly StateConfiguration _parent;

        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.TransitionConfiguration<TDerivedTrigger>
            _inner;

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

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<TState, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(
                this,
                _inner.If((state, data, trigger) => predicate(state, trigger)));
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

        public ConditionalTransitionConfiguration Execute(
            Func<TState, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration ElseIf(Func<TState, TTrigger, bool> predicate)
        {
            _inner.ElseIf((state, data, trigger) => predicate(state, trigger));
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

        private readonly StateMachine<TState, TTrigger, NoData, TCommand>.ConditionalTransitionConfiguration<
            TDerivedTrigger> _inner;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration<TDerivedTrigger> parent,
            StateMachine<TState, TTrigger, NoData, TCommand>.ConditionalTransitionConfiguration<TDerivedTrigger> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute((state, data, trigger) => action(state, trigger));
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(
            Func<TState, TDerivedTrigger, bool> predicate)
        {
            _inner.ElseIf((state, data, trigger) => predicate(state, (TDerivedTrigger)trigger));
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
