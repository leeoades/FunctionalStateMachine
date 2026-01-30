using FunctionalStateMachine.Core;
using Xunit.Abstractions;

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

public class LightSwitchDemo(ITestOutputHelper output)
{
    [Fact]
    public void Demo()
    {
        var machine = LightSwitchSample.Build();
        var state = new State<LightState, NoData>(machine.InitialStateOrDefault(), NoData.Instance);

        for (int i = 0; i < 5; i++)
        {
            (state, var commands) = machine.Fire(LightTrigger.Toggle, state);
            Run(commands);
        }
        
    }

    private void Run(IReadOnlyList<LightCommand> commands)
    {
        foreach (var command in commands)
        {
            Print(command switch
            {
                LightCommand.TurnOff => "Turn Off",
                LightCommand.TurnOn => "Turn On",
                _ => throw new ArgumentOutOfRangeException()
            });
        }
    }
    
    private void Print(string s) => output.WriteLine(s);
}