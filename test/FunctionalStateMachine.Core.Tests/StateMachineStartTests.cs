namespace FunctionalStateMachine.Core.Tests;

public class StateMachineStartTests
{
    [Fact]
    public void Start_ReturnsInitialStateAndEntryCommands()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .OnEntry((State state, Data data) => new Command($"entry-{state}-{data.Value}"))
            .Build();

        var (state, data, commands) = machine.Start(new Data(5));

        Assert.Equal(State.Ready, state);
        Assert.Equal(5, data.Value);
        Assert.Single(commands);
        Assert.Equal("entry-Ready-5", ((Command)commands[0]).Message);
    }

    [Fact]
    public void Start_NoData_ReturnsInitialStateAndEntryCommands()
    {
        var machine = StateMachine<State, Trigger, NoData, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .OnEntry((State state, NoData data) => new Command($"entry-{state}"))
            .Build();

        var (state, _, commands) = machine.Start(new NoData());

        Assert.Equal(State.Ready, state);
        Assert.Single(commands);
        Assert.Equal("entry-Ready", ((Command)commands[0]).Message);
    }

    private enum State
    {
        Ready
    }

    private enum Trigger
    {
        Advance
    }

    private sealed record Data(int Value);

    private sealed record NoData;

    private abstract record CommandBase;

    private sealed record Command(string Message) : CommandBase;
}
