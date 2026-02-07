namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, TTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(data, trigger));
    }

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(data));
    }

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TState, TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(state, data));
    }

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TState, TTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(state, trigger));
    }

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(trigger));
    }

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

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TState, TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, data, trigger) => predicate(state, data));
    }

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TCommand, TConditionalConfiguration>(
        this INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TConditionalConfiguration> configuration,
        Func<TState, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, trigger) => predicate(state));
    }

    public static TConditionalConfiguration ElseIf<TState, TTrigger, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TState, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : INoDataConditionalTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.ElseIf((state, trigger) => predicate(state));
    }
}
