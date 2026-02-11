// ReSharper disable UnusedParameter.Local - Keep (state, data, trigger) arguments for clarity
namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TImmediateTransitionConfiguration Execute<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<IEnumerable<TCommand>> action)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.Execute((state, data) => action());
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
}
