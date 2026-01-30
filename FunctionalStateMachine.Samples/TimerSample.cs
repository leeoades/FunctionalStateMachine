using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class TimerSample
{
    public static StateMachine<TimerState, TimerTrigger, TimerData, TimerCommand> Build()
    {
        return StateMachine<TimerState, TimerTrigger, TimerData, TimerCommand>.Create()
            .StartWith(TimerState.Running)
            .For(TimerState.Running)
                .OnEntry(() => new TimerCommand.Log("Start"))
                .OnExit(() => new TimerCommand.Log("Stop"))
                .On(TimerTrigger.Tick)
                    .WithData(state => state.Data with { Ticks = state.Data.Ticks + 1 })
                    .Execute(state => new TimerCommand.Log($"Tick:{state.Data.Ticks + 1}"))
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

public abstract record TimerCommand
{
    public sealed record Log(string Message) : TimerCommand;
}
