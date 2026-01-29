using FunctionalStateMachine;
using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class NoDataSample
{
    public static StateMachine<LightState, LightTrigger, LightCommand> Build()
    {
        var builder = new StateMachineBuilder<LightState, LightTrigger, LightCommand>()
            .StartWith(LightState.Off);

        builder.For(LightState.Off)
            .On(LightTrigger.Toggle)
                .TransitionTo(LightState.On)
                .Execute(() => new LightCommandBase("On"));

        builder.For(LightState.On)
            .On(LightTrigger.Toggle)
                .TransitionTo(LightState.Off)
                .Execute(() => new LightCommandBase("Off"));

        return builder.Build();
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
