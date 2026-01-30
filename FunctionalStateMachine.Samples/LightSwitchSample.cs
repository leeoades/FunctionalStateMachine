using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class LightSwitchSample
{
    public static StateMachine<LightState, LightTrigger, LightCommand> Build() =>
        StateMachine<LightState, LightTrigger, LightCommand>.Create()
            .StartWith(LightState.Off)
            .For(LightState.Off)
                .On(LightTrigger.Toggle)
                    .TransitionTo(LightState.On)
                    .Execute(() => new LightCommand.TurnOn())
            .For(LightState.On)
                .On(LightTrigger.Toggle)
                    .TransitionTo(LightState.Off)
                    .Execute(() => new LightCommand.TurnOff())
            .Build();
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

public abstract record LightCommand
{
    public record TurnOn : LightCommand;
    public record TurnOff : LightCommand;
}
