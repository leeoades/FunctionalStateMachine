// ReSharper disable UnusedParameter.Local
namespace FunctionalStateMachine.Core;

public static class StateMachineBuilderExtensions
{
    private static IEnumerable<TCommand> Single<TCommand>(TCommand command) => [command];

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

    public static TStateConfiguration OnExit<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnExit((state, data) => action());
    }

    public static TStateConfiguration OnExit<TState, TTrigger, TData, TCommand, TStateConfiguration>(
        this IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration> configuration,
        Func<TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : IStateConfiguration<TState, TTrigger, TData, TCommand, TStateConfiguration>
    {
        return configuration.OnExit((state, data) => action(data));
    }

    public static TImmediateTransitionConfiguration Guard<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TData, bool> guard)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Guard((state, data) => guard(data));
    }

    public static TImmediateTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.ModifyData((state, data) => updater(data));
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute((state, data) => Single(action()));
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute((state, data) => Single(action(data)));
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TState, TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute((state, data) => Single(action(state, data)));
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute((state, data) => action());
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute((state, data) => action(data));
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TState, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute((state, data) => action(state));
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

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action()));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data, trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data, trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action());
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(data));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(state, data));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(trigger));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(data, trigger));
    }

    public static TConditional If<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(data, trigger));
    }

    public static TConditional If<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(data));
    }

    public static TConditional If<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(state, data));
    }


    public static TTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TTrigger, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data, trigger));
    }

    public static TTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data));
    }

    public static TTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.ModifyData((state, data, trigger) => updater(state, data));
    }

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

    public static TConditionalConfiguration ModifyData<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, TTrigger, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data, trigger));
    }

    public static TConditionalConfiguration ModifyData<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data));
    }

    public static TConditionalConfiguration ModifyData<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TState, TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.ModifyData((state, data, trigger) => updater(state, data));
    }

    public static TConditionalConfiguration ModifyData<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, TDerivedTrigger, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data, trigger));
    }

    public static TConditionalConfiguration ModifyData<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data));
    }

    public static TConditionalConfiguration ModifyData<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TState, TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.ModifyData((state, data, trigger) => updater(state, data));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action()));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TState, TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(trigger)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, TTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data, trigger)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TState, TData, TTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data, trigger)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action());
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(data));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TState, TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(state, data));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(trigger));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TData, TTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(data, trigger));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration> configuration,
        Func<TState, TData, TTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(state, data, trigger));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action()));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TState, TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TDerivedTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(trigger)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, TDerivedTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data, trigger)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TState, TData, TDerivedTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data, trigger)));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action());
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(data));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TState, TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(state, data));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(trigger));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(data, trigger));
    }

    public static TConditionalConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>(
        this IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration> configuration,
        Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TConditionalConfiguration : IConditionalTransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TConditionalConfiguration>
    {
        return configuration.Execute((state, data, trigger) => action(state, data, trigger));
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

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action()));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TDerivedTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TDerivedTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(data, trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TDerivedTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => Single(action(state, data, trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action());
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(data));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(state, data));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(trigger));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(data, trigger));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TDerivedTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, data, trigger) => action(state, data, trigger));
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

    public static TConditional If<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(state, data));
    }

    public static TConditional If<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TDerivedTrigger, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, data, trigger) => predicate(trigger));
    }

    public static TTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TDerivedTrigger, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data, trigger));
    }

    public static TTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.ModifyData((state, data, trigger) => updater(data));
    }

    public static TTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : ITransitionConfiguration<TState, TTrigger, TData, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.ModifyData((state, data, trigger) => updater(state, data));
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

    public static TStateConfiguration OnExit<TState, TTrigger, TCommand, TStateConfiguration>(
        this INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration>
    {
        return configuration.OnExit(state => Single(action()));
    }

    public static TStateConfiguration OnExit<TState, TTrigger, TCommand, TStateConfiguration>(
        this INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration> configuration,
        Func<TState, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration>
    {
        return configuration.OnExit(state => Single(action(state)));
    }

    public static TStateConfiguration OnExit<TState, TTrigger, TCommand, TStateConfiguration>(
        this INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TStateConfiguration : INoDataStateConfiguration<TState, TTrigger, TCommand, TStateConfiguration>
    {
        return configuration.OnExit(state => action());
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

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>(
        this INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute(state => Single(action()));
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>(
        this INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TState, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute(state => Single(action(state)));
    }

    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>(
        this INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : INoDataImmediateTransitionConfiguration<TState, TTrigger, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute(state => action());
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

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => Single(action()));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => Single(action(state)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => Single(action(trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => action());
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => action(state));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => action(trigger));
    }

    public static TConditional If<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional> configuration,
        Func<TState, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, trigger) => predicate(state));
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

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => Single(action()));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => Single(action(state)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TDerivedTrigger, TCommand> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => Single(action(trigger)));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => action());
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => action(state));
    }

    public static TTransitionConfiguration Execute<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TDerivedTrigger, IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.Execute((state, trigger) => action(trigger));
    }

    public static TConditional If<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>(
        this INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional> configuration,
        Func<TState, bool> predicate)
        where TState : notnull
        where TTrigger : notnull
        where TDerivedTrigger : TTrigger
        where TTransitionConfiguration : INoDataTransitionConfiguration<TState, TTrigger, TCommand, TDerivedTrigger, TTransitionConfiguration, TConditional>
    {
        return configuration.If((state, trigger) => predicate(state));
    }

}
