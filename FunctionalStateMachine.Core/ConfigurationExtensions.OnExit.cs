namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TStateConfiguration OnExit<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnExit((state, data) => Single(action()));
    }

    public static TStateConfiguration OnExit<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnExit((state, data) => Single(action(data)));
    }

    public static TStateConfiguration OnExit<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TState, TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnExit((state, data) => Single(action(state, data)));
    }
}
