namespace FunctionalStateMachine.Core.Tests;

public class StateMachineUnhandledTests
{
    [Fact]
    public void TryFire_ReturnsFalseWhenUnhandled()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
            .Build();

        var current = new State<State, Data>(State.Ready, new Data("x"));
        var handled = machine.TryFire(Trigger.Start, current, out var next, out var commands);

        Assert.False(handled);
        Assert.Equal(State.Ready, next.Value);
        Assert.Empty(commands);
    }

    [Fact]
    public void OnUnhandled_InvokesHandler()
    {
        var log = new List<string>();
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .OnUnhandled((trigger, state) => log.Add($"{state.Value}:{trigger}"))
            .For(State.Ready)
            .Build();

        var current = new State<State, Data>(State.Ready, new Data("x"));
        var handled = machine.TryFire(Trigger.Start, current, out _, out var commands);

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
        var current = new State<State, Data>(State.Ready, new Data("x"));
        var (next, commands) = machine.Fire(Trigger.Ping, current);

        Assert.Equal(State.Ready, next.Value);
        Assert.Empty(commands);
    }

    [Fact]
    public void Fire_ThrowsWhenUnhandled()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        Assert.Throws<InvalidOperationException>(() => machine.Fire(Trigger.Start, current));
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
