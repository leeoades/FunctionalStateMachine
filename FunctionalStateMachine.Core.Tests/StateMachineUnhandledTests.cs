using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineUnhandledTests
{
    [Fact]
    public void Fire_InvokesOnUnhandledHandler()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .OnUnhandled()
                .Execute((trigger, state) =>
                [
                    new LogCommand($"{state}:{trigger}")
                ])
            .For(State.Ready)
            .Build();

        var currentState = State.Ready;
        var currentData = new Data("x");
        var (nextState, newData, commands) = machine.Fire(Trigger.Start, currentState, currentData);

        Assert.Equal(State.Ready, nextState);
        Assert.Equal(currentData, newData);
        Assert.Single(commands);
    }

    [Fact]
    public void Fire_IgnoresTriggersWhenConfigured()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Ping)
                    .Ignore()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data("x");
        var (nextState, _, commands) = machine.Fire(Trigger.Ping, currentState, currentData);

        Assert.Equal(State.Ready, nextState);
        Assert.Empty(commands);
    }

    [Fact]
    public void Fire_ThrowsWhenUnhandled()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
            .Build();
        var currentState = State.Ready;
        var currentData = new Data("x");

        Assert.Throws<InvalidOperationException>(() => machine.Fire(Trigger.Start, currentState, currentData));
    }

    [Fact]
    public void Fire_NoData_IgnoresDerivedTriggerWhenConfigured()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.StartTrigger>()
                    .Ignore()
            .Build();

        var (nextState, commands) = machine.Fire(new Trigger.StartTrigger(), State.Ready);

        Assert.Equal(State.Ready, nextState);
        Assert.Empty(commands);
    }

    private enum State
    {
        Ready
    }

    private abstract record Trigger
    {
        public sealed record StartTrigger : Trigger;
        public sealed record PingTrigger : Trigger;

        public static readonly Trigger Start = new StartTrigger();
        public static readonly Trigger Ping = new PingTrigger();
    }

    private sealed record Data(string Id)
    {
        public static Data Initial => new(string.Empty);
    }

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
