namespace FunctionalStateMachine.Core;

internal readonly record struct NoData
{
    public static NoData Initial { get; } = new();
}
