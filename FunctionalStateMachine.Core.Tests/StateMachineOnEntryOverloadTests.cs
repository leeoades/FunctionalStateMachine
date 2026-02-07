using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineOnEntryOverloadTests
{
    [Fact]
    public void OnEntry_Overloads_ExecuteOnEntry()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .OnEntry(() => new LogCommand("Entry-0"))
                .OnEntry((Data data) => new LogCommand($"Entry-1:{data.Value}"))
                .OnEntry((State state, Data data) => new LogCommand($"Entry-2:{state}:{data.Value}"))
                .OnEntry(() => [new LogCommand("Entry-3")])
                .OnEntry((Data data) => [new LogCommand($"Entry-4:{data.Value}")])
                .On(Trigger.Advance)
                    .TransitionTo(State.Running)
            .For(State.Running)
            .Build();

        var (_, _, commands) = machine.Start(new Data(5));

        Assert.Equal(
            new[]
            {
                "Entry-0",
                "Entry-1:5",
                "Entry-2:Ready:5",
                "Entry-3",
                "Entry-4:5"
            },
            commands.OfType<LogCommand>().Select(command => command.Message).ToArray());
    }

    private enum State
    {
        Ready,
        Running
    }

    private abstract record Trigger
    {
        public sealed record AdvanceTrigger : Trigger;

        public static readonly Trigger Advance = new AdvanceTrigger();
    }

    private sealed record Data(int Value);

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
