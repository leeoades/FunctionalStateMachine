namespace FunctionalStateMachine.Core.Tests;

public class StateMachineValidationTests
{
    [Fact]
    public void Build_ThrowsWhenTransitionToUnconfiguredState()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .OnExit(() => new LogCommand("Exit:Ready"))
                .On(Trigger.Start)
                    .TransitionTo(State.Stopped)
                    .Execute(() => new LogCommand("Transition:Ready"));

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    private enum State
    {
        Ready,
        Stopped
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
