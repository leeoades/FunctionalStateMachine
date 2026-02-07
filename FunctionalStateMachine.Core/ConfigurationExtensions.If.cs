namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TConditional If<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(data, trigger));
    }

    public static TConditional If<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TDerivedTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(data, trigger));
    }

    public static TConditional If<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(data));
    }

    public static TConditional If<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, trigger) => predicate(trigger));
    }
}
