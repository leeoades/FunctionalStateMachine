using FunctionalStateMachine;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineAdditionalTests
{
    [Fact]
    public void TryFire_ReturnsFalseWhenUnhandled()
    {
        var builder = new StateMachineBuilder<State, Trigger, Data, CommandBase>();
        builder.For(State.Ready);
        var machine = builder.Build();

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
        var builder = new StateMachineBuilder<State, Trigger, Data, CommandBase>()
            .OnUnhandled((trigger, state) => log.Add($"{state.Value}:{trigger}"));

        builder.For(State.Ready);
        var machine = builder.Build();

        var current = new State<State, Data>(State.Ready, new Data("x"));
        var handled = machine.TryFire(Trigger.Start, current, out _, out var commands);

        Assert.True(handled);
        Assert.Empty(commands);
        Assert.Single(log);
    }

    [Fact]
    public void InternalTransition_DoesNotRunEntryExit()
    {
        var builder = new StateMachineBuilder<State, Trigger, Data, CommandBase>();

        builder.For(State.Ready)
            .OnEntry(() => new LogCommand("Entry"))
            .OnExit(() => new LogCommand("Exit"))
            .On(Trigger.Tick)
                .Execute(() => new LogCommand("Tick"));

        var machine = builder.Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        var (_, commands) = machine.Fire(Trigger.Tick, current);

        Assert.Single(commands);
        Assert.IsType<LogCommand>(commands[0]);
    }

    [Fact]
    public void Execute_AllowsMultipleCommands()
    {
        var builder = new StateMachineBuilder<State, Trigger, Data, CommandBase>();

        builder.For(State.Ready)
            .On(Trigger.Start)
                .TransitionTo(State.Running)
                .Execute(() =>
            [
                new LogCommand("One"),
                    new LogCommand("Two")
            ]);

        var machine = builder.Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        var (_, commands) = machine.Fire(Trigger.Start, current);

        Assert.Equal(2, commands.Count);
    }

    private enum State
    {
        Ready,
        Running
    }

    private enum Trigger
    {
        Start,
        Tick
    }

    private sealed record Data(string Id);

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
