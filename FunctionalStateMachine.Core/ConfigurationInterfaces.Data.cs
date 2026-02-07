// ReSharper disable TypeParameterCanBeVariant
namespace FunctionalStateMachine.Core;

public interface IStateConfiguration<TState, TTrigger, TData, TCommand, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : IStateConfiguration<TState, TTrigger, TData, TCommand, TSelf>
{
    TSelf OnEntry(Func<TState, TData, IEnumerable<TCommand>> action);
    TSelf OnExit(Func<TState, TData, IEnumerable<TCommand>> action);
}

public interface IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TSelf>
{
    TSelf Guard(Func<TState, TData, bool> guard);
    TSelf ModifyData(Func<TState, TData, TData> updater);
    TSelf Execute(Func<TState, TData, IEnumerable<TCommand>> action);
}

public interface ITransitionConfiguration<TState, TTrigger, TData, TCommand, TSelf, TConditional>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TSelf, TConditional>
{
    TSelf Guard(Func<TState, TData, TTrigger, bool> guard);
    TSelf ModifyData(Func<TState, TData, TTrigger, TData> updater);
    TSelf Execute(Func<TState, TData, TTrigger, IEnumerable<TCommand>> action);
    TConditional If(Func<TState, TData, TTrigger, bool> predicate);
}

public interface ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TSelf, TConditional>
    where TState : notnull
    where TTrigger : notnull
    where TDerivedTrigger : TTrigger
    where TSelf : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TSelf, TConditional>
{
    TSelf Guard(Func<TState, TData, TDerivedTrigger, bool> guard);
    TSelf ModifyData(Func<TState, TData, TDerivedTrigger, TData> updater);
    TSelf Execute(Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action);
    TConditional If(Func<TState, TData, TDerivedTrigger, bool> predicate);
}

public interface IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TSelf : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TSelf>
{
    TSelf ModifyData(Func<TState, TData, TTrigger, TData> updater);
    TSelf Execute(Func<TState, TData, TTrigger, IEnumerable<TCommand>> action);
    TSelf ElseIf(Func<TState, TData, TTrigger, bool> predicate);
}

public interface IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TSelf>
    where TState : notnull
    where TTrigger : notnull
    where TDerivedTrigger : TTrigger
    where TSelf : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TSelf>
{
    TSelf ModifyData(Func<TState, TData, TDerivedTrigger, TData> updater);
    TSelf Execute(Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action);
    TSelf ElseIf(Func<TState, TData, TDerivedTrigger, bool> predicate);
}
