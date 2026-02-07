// ReSharper disable UnusedParameter.Local - Keep (state, data, trigger) arguments for clarity
namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    public static TImmediateTransitionConfiguration ModifyData<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>(
        this IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration> configuration,
        Func<TData, TData> updater)
        where TState : notnull
        where TTrigger : notnull
        where TImmediateTransitionConfiguration : IImmediateTransitionConfiguration<TState, TTrigger, TData, TCommand, TImmediateTransitionConfiguration>
    {
        return configuration.ModifyData((state, data) => updater(data));
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
}
