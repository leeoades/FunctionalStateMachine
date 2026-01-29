using FunctionalStateMachine;

namespace FunctionalStateMachine.Samples;

public static class InternalTransitionSample
{
    public static StateMachine<TimerState, TimerTrigger, TimerData, TimerCommand> Build()
    {
        var builder = new StateMachineBuilder<TimerState, TimerTrigger, TimerData, TimerCommand>()
            .StartWith(TimerState.Running);

        builder.For(TimerState.Running)
            .OnEntry(state => new TimerLogCommand("Start"))
            .OnExit(state => new TimerLogCommand("Stop"))
            .On(TimerTrigger.Tick)
                .WithData((state, trigger) => state.Data with { Ticks = state.Data.Ticks + 1 })
                .Execute((state, trigger) => new TimerLogCommand($"Tick:{state.Data.Ticks + 1}"));

        builder.For(TimerState.Paused)
            .On(TimerTrigger.Resume)
                .TransitionTo(TimerState.Running);

        return builder.Build();
    }
}

public enum TimerState
{
    Running,
    Paused
}

public enum TimerTrigger
{
    Tick,
    Resume
}

public sealed record TimerData(int Ticks);

public abstract record TimerCommand;

public sealed record TimerLogCommand(string Message) : TimerCommand;
