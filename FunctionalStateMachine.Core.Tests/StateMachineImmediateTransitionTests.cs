namespace FunctionalStateMachine.Core.Tests;

public class StateMachineImmediateTransitionTests
{
    [Fact]
    public void Start_AppliesEntryAndImmediateTransition()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Starting)
            .For(State.Starting)
                .OnEntry(data => new Command("enter-starting"))
                .OnExit(data => new Command("exit-starting"))
                .Immediately()
                    .TransitionTo(State.Waiting)
                    .Done()
            .For(State.Waiting)
                .OnEntry(data => new Command("enter-waiting"))
            .Build();

        var (state, _, commands) = machine.Start(Data.Initial);

        Assert.Equal(State.Waiting, state);
        Assert.Equal(
            ["enter-starting", "exit-starting", "enter-waiting"],
            commands.Select(command => command.Message).ToArray());
    }

    [Fact]
    public void Fire_AppliesImmediateTransitionsAfterEntry()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Starting)
            .For(State.Starting)
                .On(Trigger.Next)
                    .TransitionTo(State.Waiting)
            .For(State.Waiting)
                .OnEntry(data => new Command("enter-waiting"))
                .Immediately()
                    .TransitionTo(State.Ready)
                    .Done()
            .For(State.Ready)
                .OnEntry(data => new Command("enter-ready"))
            .Build();

        var (nextState, _, commands) = machine.Fire(Trigger.Next, State.Starting, Data.Initial);

        Assert.Equal(State.Ready, nextState);
        Assert.Equal(
            ["enter-waiting", "enter-ready"],
            commands.Select(command => command.Message).ToArray());
    }

    [Fact]
    public void ImmediateTransitionGuard_CanBlockTransition()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Starting)
            .For(State.Starting)
                .Immediately()
                    .Guard(data => data.Value == "go")
                    .TransitionTo(State.Waiting)
                    .Done()
            .For(State.Waiting)
            .Build();

        var (state, _, commands) = machine.Start(new Data("stop"));

        Assert.Equal(State.Starting, state);
        Assert.Empty(commands);
    }

    [Fact]
    public void ImmediateTransitionLoop_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, Command>.Create()
                .StartWith(State.Starting)
                .For(State.Starting)
                    .Immediately()
                        .TransitionTo(State.Waiting)
                        .Done()
                .For(State.Waiting)
                    .Immediately()
                        .TransitionTo(State.Starting)
                        .Done()
                .For(State.Ready)
                .Build();
        });

        Assert.Contains("infinite loop", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private enum State
    {
        Starting,
        Waiting,
        Ready
    }

    private enum Trigger
    {
        Next
    }

    private sealed record Data(string Value)
    {
        public static Data Initial => new(string.Empty);
    }

    private sealed record Command(string Message);
}
