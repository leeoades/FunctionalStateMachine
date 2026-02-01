namespace FunctionalStateMachine.Core.Tests;

public class StateMachineTriggerTests
{
    [Fact]
    public void DerivedTrigger_MatchesByType()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.WithIdTrigger>()
                    .Execute((Data data, Trigger.WithIdTrigger trigger) => new LogCommand(trigger.Id))
            .Build();
        var currentState = State.Ready;
        var currentData = new Data("original");

        var (_, _, commands) = machine.Fire(Trigger.WithId("new-id"), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("new-id", ((LogCommand)commands[0]).Message);
    }

    private enum State
    {
        Ready
    }

    private abstract record Trigger
    {
        public sealed record WithIdTrigger(string Id) : Trigger;

        public static Trigger WithId(string id) => new WithIdTrigger(id);
    }

    private sealed record Data(string Id)
    {
        public static Data Initial => new(string.Empty);
    }

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
