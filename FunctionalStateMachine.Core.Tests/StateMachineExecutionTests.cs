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
        var currentState = State.Ready;
        var currentData = new Data("x");

        var (_, _, commands) = machine.Fire(Trigger.Start, currentState, currentData);

        Assert.Equal(2, commands.Count);
    }

    [Fact]
    public void Execute_UsesUpdatedDataFromModifyData()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Start)
                    .ModifyData(data => data with { Id = "updated" })
                    .Execute(data => new LogCommand(data.Id))
            .Build();
        var currentState = State.Ready;
        var currentData = new Data("original");

        var (_, _, commands) = machine.Fire(Trigger.Start, currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("updated", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void Execute_UsesDataBasedOnOrdering()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Start)
                    .Execute(data => new LogCommand($"Before:{data.Id}"))
                    .ModifyData(data => data with { Id = "updated" })
                    .Execute(data => new LogCommand($"After:{data.Id}"))
            .Build();
        var currentState = State.Ready;
        var currentData = new Data("original");

        var (_, _, commands) = machine.Fire(Trigger.Start, currentState, currentData);

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
                    .Execute((State state, Data data) => new LogCommand($"State:{state}"))
                    .Execute((state, data, trigger) => new LogCommand($"Both:{state}:{trigger}"))
            .Build();
        var currentState = State.Ready;
        var currentData = new Data("x");

        var (_, _, commands) = machine.Fire(Trigger.Start, currentState, currentData);

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
