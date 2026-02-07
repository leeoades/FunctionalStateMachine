// ReSharper disable UnusedTupleComponentInReturnValue
// ReSharper disable UnusedMethodReturnValue.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local

namespace FunctionalStateMachine.Core;

public sealed class StateMachineBuilder<TState, TTrigger, TData, TCommand>
    where TState : notnull
    where TTrigger : notnull
{
    private readonly StateMachine<TState, TTrigger, TData, TCommand> _machine = new();
    private bool _skipAnalysis;

    public StateMachineBuilder<TState, TTrigger, TData, TCommand> StartWith(TState state)
    {
        _machine.StartWith(state);
        return this;
    }

    public UnhandledConfiguration OnUnhandled()
    {
        return new UnhandledConfiguration(this);
    }

    public sealed class UnhandledConfiguration
    {
        private readonly StateMachineBuilder<TState, TTrigger, TData, TCommand> _builder;

        internal UnhandledConfiguration(StateMachineBuilder<TState, TTrigger, TData, TCommand> builder)
        {
            _builder = builder;
        }

        public StateMachineBuilder<TState, TTrigger, TData, TCommand> Ignore()
        {
            _builder._machine.OnUnhandled((_, _) => []);
            return _builder;
        }

        public StateMachineBuilder<TState, TTrigger, TData, TCommand> Execute(
            Func<TTrigger, TState, IEnumerable<TCommand>> handler)
        {
            _builder._machine.OnUnhandled(handler);
            return _builder;
        }
    }

    public StateConfiguration For(TState state)
    {
        return new StateConfiguration(this, _machine.For(state));
    }

    /// <summary>
    /// Skip static analysis when building. Use with caution - analysis catches real configuration errors.
    /// </summary>
    public StateMachineBuilder<TState, TTrigger, TData, TCommand> SkipAnalysis()
    {
        _skipAnalysis = true;
        return this;
    }

    public StateMachine<TState, TTrigger, TData, TCommand> Build()
    {
        _machine.Validate(skipAnalysis: _skipAnalysis);
        return _machine;
    }

    public sealed class StateConfiguration
        : IStateConfiguration<TState, TTrigger, TData, TCommand, StateConfiguration>
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

        public StateConfiguration OnEntry(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public ImmediateTransitionConfiguration Immediately()
        {
            return new ImmediateTransitionConfiguration(this, _inner.Immediately());
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
            return _builder.For(state);
        }

        public StateMachine<TState, TTrigger, TData, TCommand> Build()
        {
            return _builder.Build();
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
    }

    public sealed class ImmediateTransitionConfiguration
        : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, ImmediateTransitionConfiguration>
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, TData, TCommand>.ImmediateTransitionConfiguration _inner;

        internal ImmediateTransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, TData, TCommand>.ImmediateTransitionConfiguration inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ImmediateTransitionConfiguration TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public ImmediateTransitionConfiguration Guard(Func<TState, TData, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public ImmediateTransitionConfiguration ModifyData(Func<TState, TData, TData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TState, TData, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public StateConfiguration Done()
        {
            return _parent;
        }
    }

    public sealed class TransitionConfiguration
        : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TransitionConfiguration,
            ConditionalTransitionConfiguration>
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

        public TransitionConfiguration Guard(Func<TState, TData, TTrigger, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration ModifyData(Func<TState, TData, TTrigger, TData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TData, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration If(Func<TState, TData, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _inner.If(predicate));
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

        public StateMachine<TState, TTrigger, TData, TCommand> Build()
        {
            return _parent.Build();
        }
    }

    public sealed class TransitionConfiguration<TDerivedTrigger>
        : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger,
            TransitionConfiguration<TDerivedTrigger>, ConditionalTransitionConfiguration<TDerivedTrigger>>
        where TDerivedTrigger : TTrigger
    {
        private readonly StateConfiguration _parent;

        private readonly StateMachine<TState, TTrigger, TData, TCommand>.TransitionConfiguration<TDerivedTrigger>
            _inner;

        internal TransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, TData, TCommand>.TransitionConfiguration<TDerivedTrigger> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public TransitionConfiguration<TDerivedTrigger> TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(
            Func<TState, TData, TDerivedTrigger, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<TState, TData, TDerivedTrigger, TData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<TState, TData, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(this, _inner.If(predicate));
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

        public StateMachine<TState, TTrigger, TData, TCommand> Build()
        {
            return _parent.Build();
        }
    }

    public sealed class ConditionalTransitionConfiguration
        : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, ConditionalTransitionConfiguration>
    {
        private readonly TransitionConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, TData, TCommand>.ConditionalTransitionConfiguration _inner;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration parent,
            StateMachine<TState, TTrigger, TData, TCommand>.ConditionalTransitionConfiguration inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ConditionalTransitionConfiguration ModifyData(Func<TState, TData, TTrigger, TData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ConditionalTransitionConfiguration Execute(
            Func<TState, TData, TTrigger, IEnumerable<TCommand>> action)
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

        public ConditionalTransitionConfiguration ElseIf(Func<TState, TData, TTrigger, bool> predicate)
        {
            _inner.ElseIf(predicate);
            return this;
        }

        public TransitionConfiguration Done()
        {
            _inner.Done();
            return _parent;
        }
    }

    public sealed class ConditionalTransitionConfiguration<TDerivedTrigger>
        : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger,
            ConditionalTransitionConfiguration<TDerivedTrigger>>
        where TDerivedTrigger : TTrigger
    {
        private readonly TransitionConfiguration<TDerivedTrigger> _parent;

        private readonly StateMachine<TState, TTrigger, TData, TCommand>.ConditionalTransitionConfiguration<
            TDerivedTrigger> _inner;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration<TDerivedTrigger> parent,
            StateMachine<TState, TTrigger, TData, TCommand>.ConditionalTransitionConfiguration<TDerivedTrigger> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ModifyData(
            Func<TState, TData, TDerivedTrigger, TData> updater)
        {
            _inner.ModifyData(updater);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> ElseIf(
            Func<TState, TData, TDerivedTrigger, bool> predicate)
        {
            _inner.ElseIf(predicate);
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

    public UnhandledConfiguration OnUnhandled()
    {
        return new UnhandledConfiguration(this);
    }

    public sealed class UnhandledConfiguration
    {
        private readonly StateMachineBuilder<TState, TTrigger, TCommand> _builder;

        internal UnhandledConfiguration(StateMachineBuilder<TState, TTrigger, TCommand> builder)
        {
            _builder = builder;
        }

        public StateMachineBuilder<TState, TTrigger, TCommand> Ignore()
        {
            _builder._machine.OnUnhandled((_, _) => []);
            return _builder;
        }

        public StateMachineBuilder<TState, TTrigger, TCommand> Execute(
            Func<TTrigger, TState, IEnumerable<TCommand>> handler)
        {
            _builder._machine.OnUnhandled(handler);
            return _builder;
        }

        public StateMachineBuilder<TState, TTrigger, TCommand> Execute(
            Func<TTrigger, TState, TCommand> handler)
        {
            _builder._machine.OnUnhandled((trigger, state) => [handler(trigger, state)]);
            return _builder;
        }
    }

    public StateConfiguration For(TState state)
    {
        return new StateConfiguration(this, _machine.For(state));
    }

    public StateMachine<TState, TTrigger, TCommand> Build()
    {
        _machine.Validate();
        return _machine;
    }

    public sealed class StateConfiguration
        : INoDataStateConfiguration<TState, TTrigger, TCommand, StateConfiguration>
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

        public StateConfiguration OnEntry(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.OnEntry(action);
            return this;
        }

        public StateConfiguration OnExit(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.OnExit(action);
            return this;
        }

        public ImmediateTransitionConfiguration Immediately()
        {
            return new ImmediateTransitionConfiguration(this, _inner.Immediately());
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
            return _builder.For(state);
        }

        public StateMachine<TState, TTrigger, TCommand> Build()
        {
            return _builder.Build();
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
    }

    public sealed class ImmediateTransitionConfiguration
        : INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, ImmediateTransitionConfiguration>
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, TCommand>.ImmediateTransitionConfiguration _inner;

        internal ImmediateTransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, TCommand>.ImmediateTransitionConfiguration inner)
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
            _inner.Guard(guard);
            return this;
        }

        public ImmediateTransitionConfiguration Execute(Func<TState, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public StateConfiguration Done()
        {
            return _parent;
        }
    }

    public sealed class TransitionConfiguration
        : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TransitionConfiguration,
            ConditionalTransitionConfiguration>
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

        public TransitionConfiguration Guard(Func<TState, TTrigger, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration Execute(Func<TState, TTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }

        public ConditionalTransitionConfiguration If(Func<TState, TTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration(this, _inner.If(predicate));
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

        public StateMachine<TState, TTrigger, TCommand> Build()
        {
            return _parent.Build();
        }
    }

    public sealed class TransitionConfiguration<TDerivedTrigger>
        : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger,
            TransitionConfiguration<TDerivedTrigger>, ConditionalTransitionConfiguration<TDerivedTrigger>>
        where TDerivedTrigger : TTrigger
    {
        private readonly StateConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, TCommand>.TransitionConfiguration<TDerivedTrigger> _inner;

        internal TransitionConfiguration(
            StateConfiguration parent,
            StateMachine<TState, TTrigger, TCommand>.TransitionConfiguration<TDerivedTrigger> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public TransitionConfiguration<TDerivedTrigger> TransitionTo(TState state)
        {
            _inner.TransitionTo(state);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Guard(
            Func<TState, TDerivedTrigger, bool> guard)
        {
            _inner.Guard(guard);
            return this;
        }

        public TransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action)
        {
            _inner.Execute(action);
            return this;
        }


        public ConditionalTransitionConfiguration<TDerivedTrigger> If(
            Func<TState, TDerivedTrigger, bool> predicate)
        {
            return new ConditionalTransitionConfiguration<TDerivedTrigger>(this, _inner.If(predicate));
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

        public StateMachine<TState, TTrigger, TCommand> Build()
        {
            return _parent.Build();
        }
    }

    public sealed class ConditionalTransitionConfiguration
        : INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, ConditionalTransitionConfiguration>
    {
        private readonly TransitionConfiguration _parent;
        private readonly StateMachine<TState, TTrigger, TCommand>.ConditionalTransitionConfiguration _inner;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration parent,
            StateMachine<TState, TTrigger, TCommand>.ConditionalTransitionConfiguration inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ConditionalTransitionConfiguration Execute(
            Func<TState, TTrigger, IEnumerable<TCommand>> action)
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

    public sealed class ConditionalTransitionConfiguration<TDerivedTrigger>
        : INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger,
            ConditionalTransitionConfiguration<TDerivedTrigger>>
        where TDerivedTrigger : TTrigger
    {
        private readonly TransitionConfiguration<TDerivedTrigger> _parent;

        private readonly StateMachine<TState, TTrigger, TCommand>.ConditionalTransitionConfiguration<TDerivedTrigger>
            _inner;

        internal ConditionalTransitionConfiguration(
            TransitionConfiguration<TDerivedTrigger> parent,
            StateMachine<TState, TTrigger, TCommand>.ConditionalTransitionConfiguration<TDerivedTrigger> inner)
        {
            _parent = parent;
            _inner = inner;
        }

        public ConditionalTransitionConfiguration<TDerivedTrigger> Execute(
            Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action)
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