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
}
