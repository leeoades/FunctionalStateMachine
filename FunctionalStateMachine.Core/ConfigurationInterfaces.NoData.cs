namespace FunctionalStateMachine.Core;

public interface INoDataStateConfiguration<TState, TTrigger, TCommand, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : INoDataStateConfiguration<TState, TTrigger, TCommand, TSelf>
{
}

public interface INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TSelf>
{
    TSelf Guard(Func<TState, bool> guard);
    TSelf Execute(Func<TState, IEnumerable<TCommand>> action);
}

public interface INoDataTransitionConfiguration<TState, TTrigger, TCommand, TSelf, TConditional>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TSelf, TConditional>
{
    TSelf Guard(Func<TState, TTrigger, bool> guard);
    TSelf Execute(Func<TState, TTrigger, IEnumerable<TCommand>> action);
    TConditional If(Func<TState, TTrigger, bool> predicate);
}

public interface INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TSelf, TConditional>
    where TState : notnull
    where TTrigger : notnull
    where TDerivedTrigger : TTrigger
    where TSelf : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TSelf, TConditional>
{
    TSelf Guard(Func<TState, TDerivedTrigger, bool> guard);
    TSelf Execute(Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action);
    TConditional If(Func<TState, TDerivedTrigger, bool> predicate);
}

public interface INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TSelf>
{
    TSelf Execute(Func<TState, TTrigger, IEnumerable<TCommand>> action);
    TSelf ElseIf(Func<TState, TTrigger, bool> predicate);
}

public interface INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TDerivedTrigger : TTrigger
    where TSelf : INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TSelf>
{
    TSelf Execute(Func<TState, TDerivedTrigger, IEnumerable<TCommand>> action);
    TSelf ElseIf(Func<TState, TDerivedTrigger, bool> predicate);
}
