using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class TimerSample
{
    public static StateMachine<TimerState, TimerTrigger, TimerData, TimerCommand> Build()
    {
        return StateMachine<TimerState, TimerTrigger, TimerData, TimerCommand>.Create()
            .StartWith(TimerState.Running)
            .For(TimerState.Running)
                .OnEntry(() => new TimerCommand.WriteLog("Start"))
                .OnExit(() => new TimerCommand.WriteLog("Stop"))
                .On<TimerTrigger.TickTrigger>()
                    .ModifyData(state => state.Data with { Ticks = state.Data.Ticks + 1 })
                    .Execute(state => new TimerCommand.WriteLog($"Tick:{state.Data.Ticks + 1}"))
                .For(TimerState.Paused)
                    .On<TimerTrigger.ResumeTrigger>()
                        .TransitionTo(TimerState.Running)
            .Build();
    }
}

public enum TimerState
{
    Running,
    Paused
}

public abstract record TimerTrigger
{
    public sealed record TickTrigger : TimerTrigger;
    public sealed record ResumeTrigger : TimerTrigger;

    public static readonly TimerTrigger Tick = new TickTrigger();
    public static readonly TimerTrigger Resume = new ResumeTrigger();
}

public sealed record TimerData(int Ticks);

public abstract record TimerCommand
{
    public sealed record WriteLog(string Message) : TimerCommand;
}
