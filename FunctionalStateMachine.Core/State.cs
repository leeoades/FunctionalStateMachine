namespace FunctionalStateMachine.Core;

public readonly record struct State<TState, TData>(TState Value, TData Data);
