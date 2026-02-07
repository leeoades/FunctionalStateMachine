using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineConditionalBranchTests
{
    public static TheoryData<int, string> BranchCases => new()
    {
        { 3, "Small" },
        { 7, "Medium" },
        { 15, "Large" }
    };

    [Theory]
    [MemberData(nameof(BranchCases))]
    public void IfElseIfElse_SelectsExpectedBranch(int amount, string expected)
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 10)
                        .Execute(() => new LogCommand("Large"))
                        .ElseIf((data, trigger) => trigger.Amount >= 5)
                        .Execute(() => new LogCommand("Medium"))
                        .Else()
                        .Execute(() => new LogCommand("Small"))
                        .Done()
            .Build();

        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(amount), State.Ready, new Data(0));

        Assert.Single(commands);
        Assert.Equal(expected, ((LogCommand)commands[0]).Message);
    }

    private enum State
    {
        Ready
    }

    private abstract record Trigger
    {
        public sealed record PayTrigger(int Amount) : Trigger;
    }

    private sealed record Data(int Value);

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
