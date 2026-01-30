using FunctionalStateMachine;
using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class NoDataSample
{
    public static StateMachine<LightState, LightTrigger, LightCommand> Build()
    {
        return StateMachine<LightState, LightTrigger, LightCommand>.Create()
            .StartWith(LightState.Off)
            .For(LightState.Off)
                .On(LightTrigger.Toggle)
                    .TransitionTo(LightState.On)
                    .Execute(() => new LightCommandBase("On"))
                .For(LightState.On)
                    .On(LightTrigger.Toggle)
                        .TransitionTo(LightState.Off)
                        .Execute(() => new LightCommandBase("Off"))
            .Build();
    }
}

public enum LightState
{
    Off,
    On
}

public enum LightTrigger
{
    Toggle
}

public abstract record LightCommand;

public sealed record LightCommandBase(string Name) : LightCommand;
