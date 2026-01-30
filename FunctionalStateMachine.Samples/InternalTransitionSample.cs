using FunctionalStateMachine;
using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class InternalTransitionSample
{
    public static StateMachine<TimerState, TimerTrigger, TimerData, TimerCommand> Build()
    {
        return StateMachine<TimerState, TimerTrigger, TimerData, TimerCommand>.Create()
            .StartWith(TimerState.Running)
            .For(TimerState.Running)
                .OnEntry(() => new TimerLogCommand("Start"))
                .OnExit(() => new TimerLogCommand("Stop"))
                .On(TimerTrigger.Tick)
                    .WithData(state => state.Data with { Ticks = state.Data.Ticks + 1 })
                    .Execute(state => new TimerLogCommand($"Tick:{state.Data.Ticks + 1}"))
                .For(TimerState.Paused)
                    .On(TimerTrigger.Resume)
                        .TransitionTo(TimerState.Running)
            .Build();
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
