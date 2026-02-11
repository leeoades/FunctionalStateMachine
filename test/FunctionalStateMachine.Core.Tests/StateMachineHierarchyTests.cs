namespace FunctionalStateMachine.Core.Tests;

public class StateMachineHierarchyTests
{
    [Fact]
    public void Fire_UsesParentTransitions()
    {
        var machine = StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
            .For(WorkState.Active)
                .StartsWith(WorkState.Idle)
                .OnExit((state, data) => new LogCommand($"Exit:{state}"))
                .On(WorkerTrigger.Cancel)
                    .TransitionTo(WorkState.Closed)
                    .Execute((WorkState state, WorkerData data) => new LogCommand($"Transition:{state}"))
                .For(WorkState.Idle)
                    .SubStateOf(WorkState.Active)
                    .On(WorkerTrigger.StartWork)
                        .TransitionTo(WorkState.Busy)
                .For(WorkState.Busy)
                    .SubStateOf(WorkState.Active)
                    .OnExit((state, data) => new LogCommand($"Exit:{state}"))
                .For(WorkState.Closed)
                    .OnEntry((state, data) => new LogCommand($"Entry:{state}"))
            .Build();
        var currentState = WorkState.Busy;
        var currentData = new WorkerData(0);

        var (nextState, _, commands) = machine.Fire(WorkerTrigger.Cancel, currentState, currentData);

        Assert.Equal(WorkState.Closed, nextState);
        Assert.Equal(4, commands.Count);
        Assert.Equal("Transition:Busy", ((LogCommand)commands[0]).Message);
        Assert.Equal("Exit:Busy", ((LogCommand)commands[1]).Message);
        Assert.Equal("Exit:Active", ((LogCommand)commands[2]).Message);
        Assert.Equal("Entry:Closed", ((LogCommand)commands[3]).Message);
    }

    [Fact]
    public void Fire_DoesNotRunParentExitEntryBetweenChildren()
    {
        var machine = StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
            .For(WorkState.Active)
                .StartsWith(WorkState.Idle)
                .OnEntry((state, data) => new LogCommand($"Entry:{state}"))
                .OnExit((state, data) => new LogCommand($"Exit:{state}"))
                .For(WorkState.Idle)
                    .SubStateOf(WorkState.Active)
                    .OnExit((state, data) => new LogCommand($"Exit:{state}"))
                    .On(WorkerTrigger.StartWork)
                        .TransitionTo(WorkState.Busy)
                        .Execute((WorkState state, WorkerData data) => new LogCommand($"Transition:{state}"))
                .For(WorkState.Busy)
                    .SubStateOf(WorkState.Active)
                    .OnEntry((state, data) => new LogCommand($"Entry:{state}"))
            .Build();
        var currentState = WorkState.Idle;
        var currentData = new WorkerData(0);

        var (_, _, commands) = machine.Fire(WorkerTrigger.StartWork, currentState, currentData);

        Assert.Equal(3, commands.Count);
        Assert.Equal("Transition:Idle", ((LogCommand)commands[0]).Message);
        Assert.Equal("Exit:Idle", ((LogCommand)commands[1]).Message);
        Assert.Equal("Entry:Busy", ((LogCommand)commands[2]).Message);
    }

    [Fact]
    public void Build_ThrowsWhenParentHasNoInitialSubState()
    {
        var builder = StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
            .For(WorkState.Active)
                .On(WorkerTrigger.Cancel)
                    .TransitionTo(WorkState.Closed)
                .For(WorkState.Idle)
                    .SubStateOf(WorkState.Active)
                .For(WorkState.Closed);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    private enum WorkState
    {
        Active,
        Closed,
        Idle,
        Busy
    }

    private abstract record WorkerTrigger
    {
        public sealed record StartWorkTrigger : WorkerTrigger;
        public sealed record CompleteWorkTrigger : WorkerTrigger;
        public sealed record CancelTrigger : WorkerTrigger;

        public static readonly WorkerTrigger StartWork = new StartWorkTrigger();
        public static readonly WorkerTrigger CompleteWork = new CompleteWorkTrigger();
        public static readonly WorkerTrigger Cancel = new CancelTrigger();
    }

    private sealed record WorkerData(int Count)
    {
        public static WorkerData Initial => new(0);
    }

    private abstract record TestCommand;

    private sealed record LogCommand(string Message) : TestCommand;

    #region Undefined State Tests
    
    // Note: These branches (TryGetValue returning false) are defensive code paths.
    // The validation at build time ensures all states are configured, so these
    // branches cannot be reached through normal API usage. They serve as safety
    // guards in case internal invariants are ever violated.
    
    // The following tests verify that validation correctly rejects undefined states.

    [Fact]
    public void Fire_FromUndefinedState_ThrowsInvalidOperationException()
    {
        // Validation catches undefined states at build time, but if we skip validation,
        // Fire() itself validates that the current state exists
        var machine = StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
            .SkipAnalysis()
            .StartWith(WorkState.Idle)
            .OnUnhandled().Ignore()
            .For(WorkState.Idle)
                .On(WorkerTrigger.StartWork)
                    .TransitionTo(WorkState.Busy)
            .For(WorkState.Busy)
            .Build();

        // Fire from a state that has no .For() definition
        var ex = Assert.Throws<InvalidOperationException>(() =>
            machine.Fire(WorkerTrigger.Cancel, WorkState.Closed, new WorkerData(0)));

        Assert.Contains("Closed", ex.Message);
        Assert.Contains("not configured", ex.Message);
    }

    [Fact]
    public void Build_TransitionToUndefinedState_ThrowsInvalidOperationException()
    {
        // Tests that validation catches undefined transition targets
        var ex = Assert.Throws<InvalidOperationException>(() =>
            StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
                .StartWith(WorkState.Idle)
                .For(WorkState.Idle)
                    .On(WorkerTrigger.StartWork)
                        .TransitionTo(WorkState.Closed) // Closed has no .For() definition
                .Build());

        Assert.Contains("Closed", ex.Message);
        Assert.Contains("not configured", ex.Message);
    }

    [Fact]
    public void Build_SubStateOfUndefinedParent_ThrowsInvalidOperationException()
    {
        // Tests that validation catches undefined parent states
        var ex = Assert.Throws<InvalidOperationException>(() =>
            StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
                .StartWith(WorkState.Idle)
                .For(WorkState.Idle)
                    .SubStateOf(WorkState.Active) // Active has no .For() definition
                .Build());

        Assert.Contains("Active", ex.Message);
        Assert.Contains("not configured", ex.Message);
    }

    [Fact]
    public void Build_StartsWithNonChildState_ThrowsInvalidOperationException()
    {
        // Tests that validation catches StartsWith referencing a non-child state
        var ex = Assert.Throws<InvalidOperationException>(() =>
            StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
                .StartWith(WorkState.Active)
                .For(WorkState.Active)
                    .StartsWith(WorkState.Idle)
                .For(WorkState.Idle) // Not a sub-state of Active
                .Build());

        // The error mentions Active has StartsWith but no children
        Assert.Contains("Active", ex.Message);
        Assert.Contains("StartsWith", ex.Message);
    }

    #endregion
}
