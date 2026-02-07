namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TStateConfiguration OnEntry<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry((state, data) => Single(action()));
    }

    public static TStateConfiguration OnEntry<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry((state, data) => Single(action(data)));
    }

    public static TStateConfiguration OnEntry<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TState, TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry((state, data) => Single(action(state, data)));
    }

    public static TStateConfiguration OnEntry<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry((state, data) => action());
    }

    public static TStateConfiguration OnEntry<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry((state, data) => action(data));
    }

    public static TStateConfiguration OnEntry<TState, TTrigger, TCommand, TStateConfiguration>(
        this INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry(state => Single(action()));
    }

    public static TStateConfiguration OnEntry<TState, TTrigger, TCommand, TStateConfiguration>(
        this INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration> configuration,
        Func<TState, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry(state => Single(action(state)));
    }

    public static TStateConfiguration OnEntry<TState, TTrigger, TCommand, TStateConfiguration>(
        this INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration>
    {
        return configuration.OnEntry(state => action());
    }
}
