namespace FunctionalStateMachine.Core.Tests;

public class StateMachineUnhandledTests
{
    [Fact]
    public void TryFire_ReturnsFalseWhenUnhandled()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
            .Build();

        var currentState = State.Ready;
        var currentData = new Data("x");
        var handled = machine.TryFire(Trigger.Start, currentState, currentData, out var nextState, out _, out var commands);

        Assert.False(handled);
        Assert.Equal(State.Ready, nextState);
        Assert.Empty(commands);
    }

    [Fact]
    public void OnUnhandled_InvokesHandler()
    {
        var log = new List<string>();
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .OnUnhandled((trigger, state, data) => log.Add($"{state}:{trigger}"))
            .For(State.Ready)
            .Build();

        var currentState = State.Ready;
        var currentData = new Data("x");
        var handled = machine.TryFire(Trigger.Start, currentState, currentData, out _, out _, out var commands);

        Assert.True(handled);
        Assert.Empty(commands);
        Assert.Single(log);
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
}
