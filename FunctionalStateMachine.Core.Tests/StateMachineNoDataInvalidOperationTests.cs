using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineNoDataInvalidOperationTests
{
    [Fact]
    public void InitialState_ThrowsWhenNoInitialStateConfigured()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .For(State.Ready)
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() => _ = machine.InitialState);

        Assert.Contains("initial state", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private enum State
    {
        Ready
    }

    private enum Trigger
    {
        Go
    }

    private abstract record CommandBase;
}
