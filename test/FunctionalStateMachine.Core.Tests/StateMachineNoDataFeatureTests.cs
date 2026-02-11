namespace FunctionalStateMachine.Core.Tests;

/// <summary>
/// Tests for NoData state machine features that were previously only available in the Data version.
/// </summary>
public class StateMachineNoDataFeatureTests
{
    #region Ignore Tests

    [Fact]
    public void Ignore_NonGenericTrigger_DoesNotThrow()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Ignored)
                    .Ignore()
                .On(Trigger.Step)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        var (state, commands) = machine.Fire(Trigger.Ignored, State.Ready);

        Assert.Equal(State.Ready, state);
        Assert.Empty(commands);
    }

    #endregion

    #region TransitionTo in Conditional Tests

    [Fact]
    public void ConditionalTransitionTo_NonGeneric_TransitionsToCorrectState()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Step)
                    .If((state, trigger) => true)
                        .TransitionTo(State.Done)
                        .Done()
                    .Done()
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        var (state, _) = machine.Fire(Trigger.Step, State.Ready);

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void ConditionalTransitionTo_Generic_TransitionsToCorrectState()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.StepTrigger>()
                    .If((state, trigger) => true)
                        .TransitionTo(State.Done)
                        .Done()
                    .Done()
            .For(State.Done)
                .On<Trigger.StepTrigger>()
                    .Done()
            .Build();

        var (state, _) = machine.Fire(Trigger.Step, State.Ready);

        Assert.Equal(State.Done, state);
    }

    #endregion

    #region Hierarchy Tests

    [Fact]
    public void SubStateOf_NoData_EstablishesHierarchy()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Step)
                    .TransitionTo(State.ParentState)
                    .Done()
            .For(State.ParentState)
                .StartsWith(State.ChildState)
                .On(Trigger.Step)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.ChildState)
                .SubStateOf(State.ParentState)
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        // Fire to transition to parent (resolves to child)
        var (state1, _) = machine.Fire(Trigger.Step, State.Ready);
        Assert.Equal(State.ChildState, state1);

        // Fire again - child has no transition for Step, uses parent's
        var (state2, _) = machine.Fire(Trigger.Step, State.ChildState);
        Assert.Equal(State.Done, state2);
    }

    #endregion

    #region OnEntry/OnExit Tests

    [Fact]
    public void OnEntry_NoData_ExecutesOnStateEntry()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Step)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
                .OnEntry(state => new CommandBase[] { new Command($"Entered {state}") })
                .On(Trigger.Step)
                    .Done()
            .Build();

        var (_, commands) = machine.Fire(Trigger.Step, State.Ready);

        Assert.Single(commands);
        Assert.Equal("Entered Done", ((Command)commands[0]).Message);
    }

    [Fact]
    public void OnExit_NoData_ExecutesOnStateExit()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .OnExit(state => new CommandBase[] { new Command($"Exited {state}") })
                .On(Trigger.Step)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        var (_, commands) = machine.Fire(Trigger.Step, State.Ready);

        Assert.Single(commands);
        Assert.Equal("Exited Ready", ((Command)commands[0]).Message);
    }

    #endregion

    #region Immediately Tests

    [Fact]
    public void Immediately_NoData_TransitionsWithoutTrigger()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        var (state, _) = machine.Start();

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Immediately_WithGuard_NoData_OnlyTransitionsWhenGuardPasses()
    {
        // Guard that always fails - should stay in Ready
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .Guard(state => false)
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Step)
                    .Done()
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        var (state, _) = machine.Start();

        Assert.Equal(State.Ready, state);
    }

    [Fact]
    public void Immediately_WithExecute_NoData_ExecutesCommandsOnImmediateTransition()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .Execute(state => new CommandBase[] { new Command("Immediate") })
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        var (state, commands) = machine.Start();

        Assert.Equal(State.Done, state);
        Assert.Single(commands);
        Assert.Equal("Immediate", ((Command)commands[0]).Message);
    }

    #endregion

    #region SkipAnalysis Tests

    [Fact]
    public void SkipAnalysis_NoData_BuildsWithoutValidation()
    {
        // This would normally fail validation (unreachable state)
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .SkipAnalysis()
            .For(State.Ready)
                .On(Trigger.Step)
                    .Done()
            // State.Done is never reachable - would fail validation
            .For(State.Done)
                .On(Trigger.Step)
                    .Done()
            .Build();

        Assert.Equal(State.Ready, machine.InitialState);
    }

    #endregion

    #region Test Types

    private enum State
    {
        Ready,
        Done,
        ParentState,
        ChildState
    }

    private abstract record Trigger
    {
        public sealed record StepTrigger : Trigger;
        public sealed record IgnoredTrigger : Trigger;

        public static readonly Trigger Step = new StepTrigger();
        public static readonly Trigger Ignored = new IgnoredTrigger();
    }

    private abstract record CommandBase;

    private sealed record Command(string Message) : CommandBase;

    #endregion
}
