namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TImmediateTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Guard((state, data) => guard(data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data, trigger));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TData, TTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data, trigger));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(state, data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(state, data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, TData, TTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard(guard);
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TDerivedTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data, trigger));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TData, TDerivedTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data, trigger));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TDerivedTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard(guard);
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, TData, TDerivedTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard(guard);
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(state, data));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, data, trigger) => guard(state, data));
    }

    public static TImmediateTransitionConfiguration Guard<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>(
        this INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Guard(state => guard());
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, trigger) => guard(state));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, trigger) => guard(state));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard(guard);
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, TTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard(guard);
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, trigger) => guard(state));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard((state, trigger) => guard(state));
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TDerivedTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard(guard);
    }

    public static TTransitionConfiguration Guard<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        string label,
        Func<TState, TDerivedTrigger, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Guard(guard);
    }
}
