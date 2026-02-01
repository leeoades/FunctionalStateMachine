namespace FunctionalStateMachine.Core;

public readonly record struct NoData
{
    public static readonly NoData Instance = new();
    public static NoData Initial => Instance;
};
