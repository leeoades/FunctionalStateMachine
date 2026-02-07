namespace FunctionalStateMachine.Core.Tests;

public class StateMachineGuardOverloadTests
{
    [Fact]
    public void Guard_Data_Overloads_TestAcceptAndDeny()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .OnUnhandled()
                .Ignore()
            .For(State.Ready)
                .On(Trigger.Advance)
                    .Guard((Data data, Trigger trigger) => data.Value > 0 && trigger == Trigger.Advance)
                    .Execute(() => new Command("data-trigger"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Refresh)
                    .Guard("guard-data", (Data data) => data.Value > 0)
                    .Execute(() => new Command("data-only"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Cancel)
                    .Guard((State state, Data data) => state == State.Ready && data.Value > 0)
                    .Execute(() => new Command("state-data"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Halt)
                    .Guard("guard-all", (State state, Data data, Trigger trigger) => state == State.Ready && data.Value > 0)
                    .Execute(() => new Command("state-data-trigger"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Derived)
                    .Guard((Data data) => data.Value > 0)
                    .Execute(() => new Command("data-only-derived"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.DerivedAll)
                    .Guard("guard-state-data", (State state, Data data) => state == State.Ready && data.Value > 0)
                    .Execute(() => new Command("state-data-derived"))
                    .TransitionTo(State.Done)
                    .Done()
                .On<Trigger.DerivedTrigger>()
                    .Guard((Data data, Trigger.DerivedTrigger trigger) => data.Value >= trigger.Amount)
                    .Execute(() => new Command("data-derived"))
                    .TransitionTo(State.Done)
                    .Done()
                .On<Trigger.DerivedTrigger>()
                    .Guard("guard-derived", (State state, Data data, Trigger.DerivedTrigger trigger) => state == State.Ready && data.Value >= trigger.Amount)
                    .Execute(() => new Command("state-data-derived"))
                    .TransitionTo(State.Done)
                    .Done()
                .For(State.Done)
                .Build();

        var (acceptedState, _, acceptedCommands) = machine.Fire(Trigger.Advance, State.Ready, new Data(1));
        var (deniedState, _, deniedCommands) = machine.Fire(Trigger.Advance, State.Ready, new Data(0));
        Assert.Equal(State.Done, acceptedState);
        Assert.Single(acceptedCommands);
        Assert.Equal("data-trigger", ((Command)acceptedCommands[0]).Message);
        Assert.Equal(State.Ready, deniedState);
        Assert.Empty(deniedCommands);

        var (dataOnlyAccepted, _, dataOnlyCommands) = machine.Fire(Trigger.Refresh, State.Ready, new Data(1));
        var (dataOnlyDenied, _, dataOnlyDeniedCommands) = machine.Fire(Trigger.Refresh, State.Ready, new Data(0));
        Assert.Equal(State.Done, dataOnlyAccepted);
        Assert.Single(dataOnlyCommands);
        Assert.Equal("data-only", ((Command)dataOnlyCommands[0]).Message);
        Assert.Equal(State.Ready, dataOnlyDenied);
        Assert.Empty(dataOnlyDeniedCommands);

        var (stateDataAccepted, _, stateDataCommands) = machine.Fire(Trigger.Cancel, State.Ready, new Data(1));
        var (stateDataDenied, _, stateDataDeniedCommands) = machine.Fire(Trigger.Cancel, State.Ready, new Data(0));
        Assert.Equal(State.Done, stateDataAccepted);
        Assert.Single(stateDataCommands);
        Assert.Equal("state-data", ((Command)stateDataCommands[0]).Message);
        Assert.Equal(State.Ready, stateDataDenied);
        Assert.Empty(stateDataDeniedCommands);

        var (allAccepted, _, allCommands) = machine.Fire(Trigger.Halt, State.Ready, new Data(1));
        var (allDenied, _, allDeniedCommands) = machine.Fire(Trigger.Halt, State.Ready, new Data(0));
        Assert.Equal(State.Done, allAccepted);
        Assert.Single(allCommands);
        Assert.Equal("state-data-trigger", ((Command)allCommands[0]).Message);
        Assert.Equal(State.Ready, allDenied);
        Assert.Empty(allDeniedCommands);

        var (dataOnlyDerivedAccepted, _, dataOnlyDerivedCommands) = machine.Fire(Trigger.Derived, State.Ready, new Data(1));
        var (dataOnlyDerivedDenied, _, dataOnlyDerivedDeniedCommands) = machine.Fire(Trigger.Derived, State.Ready, new Data(0));
        Assert.Equal(State.Done, dataOnlyDerivedAccepted);
        Assert.Single(dataOnlyDerivedCommands);
        Assert.Equal("data-only-derived", ((Command)dataOnlyDerivedCommands[0]).Message);
        Assert.Equal(State.Ready, dataOnlyDerivedDenied);
        Assert.Empty(dataOnlyDerivedDeniedCommands);

        var (stateDataDerivedAccepted, _, stateDataDerivedCommands) = machine.Fire(Trigger.DerivedAll, State.Ready, new Data(1));
        var (stateDataDerivedDenied, _, stateDataDerivedDeniedCommands) = machine.Fire(Trigger.DerivedAll, State.Ready, new Data(0));
        Assert.Equal(State.Done, stateDataDerivedAccepted);
        Assert.Single(stateDataDerivedCommands);
        Assert.Equal("state-data-derived", ((Command)stateDataDerivedCommands[0]).Message);
        Assert.Equal(State.Ready, stateDataDerivedDenied);
        Assert.Empty(stateDataDerivedDeniedCommands);

        var (derivedAccepted, _, derivedCommands) = machine.Fire(new Trigger.DerivedTrigger(1), State.Ready, new Data(2));
        var (derivedDenied, _, derivedDeniedCommands) = machine.Fire(new Trigger.DerivedTrigger(2), State.Ready, new Data(1));
        Assert.Equal(State.Done, derivedAccepted);
        Assert.Single(derivedCommands);
        Assert.Equal("data-derived", ((Command)derivedCommands[0]).Message);
        Assert.Equal(State.Ready, derivedDenied);
        Assert.Empty(derivedDeniedCommands);
    }

    [Fact]
    public void Guard_NoData_Overloads_TestAcceptAndDeny()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .OnUnhandled()
                .Ignore()
            .For(State.Ready)
                .On(Trigger.Advance)
                    .Guard((State state, Trigger trigger) => state == State.Ready && trigger == Trigger.Advance)
                    .Execute(() => new Command("state-trigger"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Refresh)
                    .Guard((State state) => state == State.Ready)
                    .Execute(() => new Command("state-only"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Cancel)
                    .Guard("guard-state", (State state) => state == State.Ready)
                    .Execute(() => new Command("state-label"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Halt)
                    .Guard("guard-all", (State state, Trigger trigger) => state == State.Ready && trigger == Trigger.Halt)
                    .Execute(() => new Command("state-trigger-label"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Derived)
                    .Guard((State state) => state == State.Ready)
                    .Execute(() => new Command("state-only-derived"))
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.DerivedAll)
                    .Guard("guard-derived-state", (State state) => state == State.Ready)
                    .Execute(() => new Command("state-label-derived"))
                    .TransitionTo(State.Done)
                    .Done()
                .On<Trigger.DerivedTrigger>()
                    .Guard((State state, Trigger.DerivedTrigger trigger) => state == State.Ready && trigger.Amount > 0)
                    .Execute(() => new Command("derived"))
                    .TransitionTo(State.Done)
                    .Done()
                .On<Trigger.DerivedTrigger>()
                    .Guard("guard-derived", (State state, Trigger.DerivedTrigger trigger) => state == State.Ready && trigger.Amount > 0)
                    .Execute(() => new Command("derived-label"))
                    .TransitionTo(State.Done)
                    .Done()
                .For(State.Done)
                .Build();

        var (acceptedState, acceptedCommands) = machine.Fire(Trigger.Advance, State.Ready);
        var (deniedState, deniedCommands) = machine.Fire(Trigger.Advance, State.Done);
        Assert.Equal(State.Done, acceptedState);
        Assert.Single(acceptedCommands);
        Assert.Equal("state-trigger", ((Command)acceptedCommands[0]).Message);
        Assert.Equal(State.Done, deniedState);
        Assert.Empty(deniedCommands);

        var (stateOnlyAccepted, stateOnlyCommands) = machine.Fire(Trigger.Refresh, State.Ready);
        var (stateOnlyDenied, stateOnlyDeniedCommands) = machine.Fire(Trigger.Refresh, State.Done);
        Assert.Equal(State.Done, stateOnlyAccepted);
        Assert.Single(stateOnlyCommands);
        Assert.Equal("state-only", ((Command)stateOnlyCommands[0]).Message);
        Assert.Equal(State.Done, stateOnlyDenied);
        Assert.Empty(stateOnlyDeniedCommands);

        var (stateLabelAccepted, stateLabelCommands) = machine.Fire(Trigger.Cancel, State.Ready);
        var (stateLabelDenied, stateLabelDeniedCommands) = machine.Fire(Trigger.Cancel, State.Done);
        Assert.Equal(State.Done, stateLabelAccepted);
        Assert.Single(stateLabelCommands);
        Assert.Equal("state-label", ((Command)stateLabelCommands[0]).Message);
        Assert.Equal(State.Done, stateLabelDenied);
        Assert.Empty(stateLabelDeniedCommands);

        var (stateAllAccepted, stateAllCommands) = machine.Fire(Trigger.Halt, State.Ready);
        var (stateAllDenied, stateAllDeniedCommands) = machine.Fire(Trigger.Halt, State.Done);
        Assert.Equal(State.Done, stateAllAccepted);
        Assert.Single(stateAllCommands);
        Assert.Equal("state-trigger-label", ((Command)stateAllCommands[0]).Message);
        Assert.Equal(State.Done, stateAllDenied);
        Assert.Empty(stateAllDeniedCommands);

        var (stateDerivedAccepted, stateDerivedCommands) = machine.Fire(Trigger.Derived, State.Ready);
        var (stateDerivedDenied, stateDerivedDeniedCommands) = machine.Fire(Trigger.Derived, State.Done);
        Assert.Equal(State.Done, stateDerivedAccepted);
        Assert.Single(stateDerivedCommands);
        Assert.Equal("state-only-derived", ((Command)stateDerivedCommands[0]).Message);
        Assert.Equal(State.Done, stateDerivedDenied);
        Assert.Empty(stateDerivedDeniedCommands);

        var (stateDerivedLabelAccepted, stateDerivedLabelCommands) = machine.Fire(Trigger.DerivedAll, State.Ready);
        var (stateDerivedLabelDenied, stateDerivedLabelDeniedCommands) = machine.Fire(Trigger.DerivedAll, State.Done);
        Assert.Equal(State.Done, stateDerivedLabelAccepted);
        Assert.Single(stateDerivedLabelCommands);
        Assert.Equal("state-label-derived", ((Command)stateDerivedLabelCommands[0]).Message);
        Assert.Equal(State.Done, stateDerivedLabelDenied);
        Assert.Empty(stateDerivedLabelDeniedCommands);

        var (derivedAccepted, derivedCommands) = machine.Fire(new Trigger.DerivedTrigger(1), State.Ready);
        var (derivedDenied, derivedDeniedCommands) = machine.Fire(new Trigger.DerivedTrigger(0), State.Ready);
        Assert.Equal(State.Done, derivedAccepted);
        Assert.Single(derivedCommands);
        Assert.Equal("derived", ((Command)derivedCommands[0]).Message);
        Assert.Equal(State.Ready, derivedDenied);
        Assert.Empty(derivedDeniedCommands);
    }

    private enum State
    {
        Ready,
        Done
    }

    private abstract record Trigger
    {
        public sealed record AdvanceTrigger : Trigger;
        public sealed record RefreshTrigger : Trigger;
        public sealed record CancelTrigger : Trigger;
        public sealed record HaltTrigger : Trigger;
        public sealed record DerivedTrigger(int Amount) : Trigger;
        public sealed record DerivedGuardTrigger : Trigger;
        public sealed record DerivedStateGuardTrigger : Trigger;

        public static readonly Trigger Advance = new AdvanceTrigger();
        public static readonly Trigger Refresh = new RefreshTrigger();
        public static readonly Trigger Cancel = new CancelTrigger();
        public static readonly Trigger Halt = new HaltTrigger();
        public static readonly Trigger Derived = new DerivedGuardTrigger();
        public static readonly Trigger DerivedAll = new DerivedStateGuardTrigger();
    }

    private sealed record Data(int Value);

    private abstract record CommandBase;

    private sealed record Command(string Message) : CommandBase;
}
