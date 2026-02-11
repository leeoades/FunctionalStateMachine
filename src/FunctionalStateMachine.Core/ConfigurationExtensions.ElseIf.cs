// ReSharper disable UnusedParameter.Local - Keep (state, data, trigger) arguments for clarity
namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, TDerivedTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(data, trigger));
    }

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(data));
    }
}
