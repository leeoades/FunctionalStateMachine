namespace FunctionalStateMachine.Core.Tests;

public class StateMachineModifyDataTests
{
    [Fact]
    public void ModifyData_ImmediateTransition_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .ModifyData((State state, Data data) => data with { Value = data.Value + 1 })
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (state, data, commands) = machine.Start(new Data(2));

        Assert.Equal(State.Done, state);
        Assert.Equal(3, data.Value);
        Assert.Empty(commands);
    }

    [Fact]
    public void ModifyData_TransitionWithTriggerAndState_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Advance)
                    .ModifyData((State state, Data data, Trigger trigger) => data with { Value = data.Value + 5 })
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, data, _) = machine.Fire(Trigger.Advance, State.Ready, new Data(1));

        Assert.Equal(State.Done, state);
        Assert.Equal(6, data.Value);
    }

    [Fact]
    public void ModifyData_ConditionalBranch_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.DerivedTrigger>()
                    .If((Data data, Trigger.DerivedTrigger trigger) => trigger.Amount > 0)
                        .ModifyData((Data data, Trigger.DerivedTrigger trigger) => data with { Value = data.Value + trigger.Amount })
                        .Done()
            .Build();

        var (state, data, _) = machine.Fire(new Trigger.DerivedTrigger(4), State.Ready, new Data(1));

        Assert.Equal(State.Ready, state);
        Assert.Equal(5, data.Value);
    }

    [Fact]
    public void ModifyData_NoData_IsNotAvailable()
    {
        var machine = StateMachine<State, Trigger, NoData, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Advance)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, commands) = machine.Fire(Trigger.Advance, State.Ready, new NoData());

        Assert.Equal(State.Done, state);
        Assert.Empty(commands);
    }

    private enum State
    {
        Ready,
        Done
    }

    private abstract record Trigger
    {
        public sealed record AdvanceTrigger : Trigger;
        public sealed record DerivedTrigger(int Amount) : Trigger;

        public static readonly Trigger Advance = new AdvanceTrigger();
    }

    private sealed record Data(int Value);

    private sealed record NoData;

    private abstract record CommandBase;
}
