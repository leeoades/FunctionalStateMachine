namespace FunctionalStateMachine.Core.Tests;

public class StateMachineExecutionTests
{
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
            .For(State.Running)
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        var (_, commands) = machine.Fire(Trigger.Start, current);

        Assert.Equal(2, commands.Count);
    }

    [Fact]
    public void Execute_UsesUpdatedDataFromModifyData()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Start)
                    .ModifyData(state => state.Data with { Id = "updated" })
                    .Execute(state => new LogCommand(state.Data.Id))
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("original"));

        var (_, commands) = machine.Fire(Trigger.Start, current);

        Assert.Single(commands);
        Assert.Equal("updated", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void Execute_UsesDataBasedOnOrdering()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Start)
                    .Execute(state => new LogCommand($"Before:{state.Data.Id}"))
                    .ModifyData(state => state.Data with { Id = "updated" })
                    .Execute(state => new LogCommand($"After:{state.Data.Id}"))
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("original"));

        var (_, commands) = machine.Fire(Trigger.Start, current);

        Assert.Equal("Before:original", ((LogCommand)commands[0]).Message);
        Assert.Equal("After:updated", ((LogCommand)commands[1]).Message);
    }

    [Fact]
    public void Execute_OverloadsSupportMissingArguments()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Start)
                    .Execute(() => new LogCommand("NoArgs"))
                    .Execute((Trigger trigger) => new LogCommand($"Trigger:{trigger}"))
                    .Execute(state => new LogCommand($"State:{state.Value}"))
                    .Execute((state, trigger) => new LogCommand($"Both:{state.Value}:{trigger}"))
            .Build();
        var current = new State<State, Data>(State.Ready, new Data("x"));

        var (_, commands) = machine.Fire(Trigger.Start, current);

        Assert.Equal(4, commands.Count);
    }

    private enum State
    {
        Ready,
        Running
    }

    private abstract record Trigger
    {
        public sealed record StartTrigger : Trigger;

        public static readonly Trigger Start = new StartTrigger();
    }

    private sealed record Data(string Id)
    {
        public static Data Initial => new(string.Empty);
    }

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
