namespace FunctionalStateMachine.Core.Tests;

public class StateMachineHierarchyTests
{
    [Fact]
    public void Fire_UsesParentTransitions()
    {
        var machine = StateMachine<WorkState, WorkerTrigger, WorkerData, TestCommand>.Create()
            .For(WorkState.Active)
                .StartsWith(WorkState.Idle)
                .OnExit(state => new LogCommand($"Exit:{state.Value}"))
                .On(WorkerTrigger.Cancel)
                    .TransitionTo(WorkState.Closed)
                    .Execute(state => new LogCommand($"Transition:{state.Value}"))
                .For(WorkState.Idle)
                    .SubStateOf(WorkState.Active)
                    .On(WorkerTrigger.StartWork)
                        .TransitionTo(WorkState.Busy)
                .For(WorkState.Busy)
                    .SubStateOf(WorkState.Active)
                    .OnExit(state => new LogCommand($"Exit:{state.Value}"))
                .For(WorkState.Closed)
                    .OnEntry(state => new LogCommand($"Entry:{state.Value}"))
            .Build();
        var current = new State<WorkState, WorkerData>(WorkState.Busy, new WorkerData(0));

        var (next, commands) = machine.Fire(WorkerTrigger.Cancel, current);

        Assert.Equal(WorkState.Closed, next.Value);
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
                .OnEntry(state => new LogCommand($"Entry:{state.Value}"))
                .OnExit(state => new LogCommand($"Exit:{state.Value}"))
                .For(WorkState.Idle)
                    .SubStateOf(WorkState.Active)
                    .OnExit(state => new LogCommand($"Exit:{state.Value}"))
                    .On(WorkerTrigger.StartWork)
                        .TransitionTo(WorkState.Busy)
                        .Execute(state => new LogCommand($"Transition:{state.Value}"))
                .For(WorkState.Busy)
                    .SubStateOf(WorkState.Active)
                    .OnEntry(state => new LogCommand($"Entry:{state.Value}"))
            .Build();
        var current = new State<WorkState, WorkerData>(WorkState.Idle, new WorkerData(0));

        var (_, commands) = machine.Fire(WorkerTrigger.StartWork, current);

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
