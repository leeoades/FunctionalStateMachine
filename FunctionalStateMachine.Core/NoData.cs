namespace FunctionalStateMachine.Core;

internal readonly record struct NoData
{
    public static readonly NoData Instance = new();
    public static NoData Initial => Instance;
};
