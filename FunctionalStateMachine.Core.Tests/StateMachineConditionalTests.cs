namespace FunctionalStateMachine.Core.Tests;

public class StateMachineConditionalTests
{
    [Fact]
    public void If_TrueBranchExecutesStepsInOrder()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((state, trigger) => trigger.Amount >= 10m)
                        .ModifyData((state, trigger) => state.Data with { Total = state.Data.Total + trigger.Amount })
                        .Execute(state => new LogCommand($"Paid:{state.Data.Total}"))
                        .Done()
            .Build();
        var current = new State<State, Data>(State.Ready, new Data(0m));

        var (_, commands) = machine.Fire(new Trigger.PayTrigger(12m), current);

        Assert.Single(commands);
        Assert.Equal("Paid:12", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void If_ElseBranchExecutesWhenPredicateIsFalse()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((state, trigger) => trigger.Amount >= 10m)
                        .Execute(() => new LogCommand("Accepted"))
                        .Else()
                        .Execute((state, trigger) => new LogCommand($"Need:{10m - trigger.Amount}"))
                        .Done()
            .Build();
        var current = new State<State, Data>(State.Ready, new Data(0m));

        var (_, commands) = machine.Fire(new Trigger.PayTrigger(7m), current);

        Assert.Single(commands);
        Assert.Equal("Need:3", ((LogCommand)commands[0]).Message);
    }

    private enum State
    {
        Ready
    }

    private abstract record Trigger
    {
        public sealed record PayTrigger(decimal Amount) : Trigger;
    }

    private sealed record Data(decimal Total)
    {
        public static Data Initial => new(0m);
    }

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
