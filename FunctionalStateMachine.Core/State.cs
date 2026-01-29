namespace FunctionalStateMachine;

public readonly record struct State<TState, TData>(TState Value, TData Data);

public readonly record struct SubState<TState, TData>(TState Value, TData Data);
