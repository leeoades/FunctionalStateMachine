namespace FunctionalStateMachine.Core.Tests;

public class StateMachineAdditionalTests
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
    public void InternalTransition_DoesNotRunEntryExit()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .OnEntry(() => new LogCommand("Entry"))
                .OnExit(() => new LogCommand("Exit"))
                .On(Trigger.Tick)
                    .Execute(() => new LogCommand("Tick"))
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        var (_, commands) = machine.Fire(Trigger.Tick, current);

        Assert.Single(commands);
        Assert.IsType<LogCommand>(commands[0]);
    }

    [Fact]
    public void Execute_AllowsMultipleCommands()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Start)
                    .TransitionTo(State.Running)
                    .Execute(() =>
                    [
                        new LogCommand("One"),
                        new LogCommand("Two")
                    ])
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        var (_, commands) = machine.Fire(Trigger.Start, current);

        Assert.Equal(2, commands.Count);
    }

    [Fact]
    public void Execute_UsesUpdatedDataFromWithData()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Start)
                    .WithData(state => state.Data with { Id = "updated" })
                    .Execute(state => new LogCommand(state.Data.Id))
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("original"));

        var (_, commands) = machine.Fire(Trigger.Start, current);

        Assert.Single(commands);
        Assert.Equal("updated", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void DerivedTrigger_MatchesByType()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.WithIdTrigger>()
                    .Execute((state, trigger) => new LogCommand(trigger.Id))
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("original"));

        var (_, commands) = machine.Fire(Trigger.WithId("new-id"), current);

        Assert.Single(commands);
        Assert.Equal("new-id", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void TransitionToUnconfiguredState_DoesNotRunEntryActions()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .OnExit(() => new LogCommand("Exit:Ready"))
                .On(Trigger.Start)
                    .TransitionTo(State.Stopped)
                    .Execute(() => new LogCommand("Transition:Ready"))
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        var (next, commands) = machine.Fire(Trigger.Start, current);

        Assert.Equal(State.Stopped, next.Value);
        Assert.Equal(2, commands.Count);
        Assert.Equal("Exit:Ready", ((LogCommand)commands[0]).Message);
        Assert.Equal("Transition:Ready", ((LogCommand)commands[1]).Message);
    }

    private enum State
    {
        Ready,
        Running,
        Stopped
    }

    private abstract record Trigger
    {
        public sealed record StartTrigger : Trigger;
        public sealed record TickTrigger : Trigger;
        public sealed record WithIdTrigger(string Id) : Trigger;

        public static readonly Trigger Start = new StartTrigger();
        public static readonly Trigger Tick = new TickTrigger();

        public static Trigger WithId(string id) => new WithIdTrigger(id);
    }

    private sealed record Data(string Id);

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
