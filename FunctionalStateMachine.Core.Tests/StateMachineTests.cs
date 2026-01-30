namespace FunctionalStateMachine.Core.Tests;

public class StateMachineTests
{
    [Fact]
    public void Fire_ReturnsNewStateAndCommands()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .StartWith(OrderState.Created)
            .For(OrderState.Created)
                .On(OrderTrigger.Pay)
                    .TransitionTo(OrderState.Paid)
                    .Execute(state => new ChargeCommand(state.Data.OrderId))
            .Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-100", "none"));
        var (next, commands) = machine.Fire(OrderTrigger.Pay, current);

        Assert.Equal(OrderState.Paid, next.Value);
        Assert.Equal("A-100", next.Data.OrderId);
        Assert.Single(commands);
        Assert.IsType<ChargeCommand>(commands[0]);
    }

    [Fact]
    public void Fire_AppendsExitTransitionEntryCommands()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
                .OnExit(state => new LogCommand($"Exit:{state.Value}"))
                .On(OrderTrigger.Pay)
                    .TransitionTo(OrderState.Paid)
                    .Execute(state => new LogCommand($"Transition:{state.Value}"))
                .For(OrderState.Paid)
                    .OnEntry(state => new LogCommand($"Entry:{state.Value}"))
            .Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-200", "none"));
        var (_, commands) = machine.Fire(OrderTrigger.Pay, current);

        Assert.Equal(3, commands.Count);
        Assert.IsType<LogCommand>(commands[0]);
        Assert.IsType<LogCommand>(commands[1]);
        Assert.IsType<LogCommand>(commands[2]);
    }

    [Fact]
    public void Fire_UsesGuardsInOrder()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
                .On(OrderTrigger.Pay)
                    .Guard((state, trigger) => state.Data.OrderId.StartsWith("B"))
                    .TransitionTo(OrderState.Cancelled)
                .On(OrderTrigger.Pay)
                    .Guard((state, trigger) => state.Data.OrderId.StartsWith("A"))
                    .TransitionTo(OrderState.Paid)
            .Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-300", "none"));
        var (next, _) = machine.Fire(OrderTrigger.Pay, current);

        Assert.Equal(OrderState.Paid, next.Value);
    }

    [Fact]
    public void Fire_UpdatesDataWhenConfigured()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
                .On(OrderTrigger.Pay)
                    .WithData(state => state.Data with { LastEvent = "paid" })
                    .TransitionTo(OrderState.Paid)
            .Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-400", "none"));
        var (next, _) = machine.Fire(OrderTrigger.Pay, current);

        Assert.Equal("paid", next.Data.LastEvent);
    }

    [Fact]
    public void Fire_IgnoresTriggersWhenConfigured()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
                .On(OrderTrigger.Cancel)
                    .Ignore()
            .Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-500", "none"));
        var (next, commands) = machine.Fire(OrderTrigger.Cancel, current);

        Assert.Equal(OrderState.Created, next.Value);
        Assert.Empty(commands);
    }

    [Fact]
    public void Fire_ThrowsWhenUnhandled()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
            .Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-600", "none"));

        Assert.Throws<InvalidOperationException>(() => machine.Fire(OrderTrigger.Pay, current));
    }

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
        Assert.Equal("Exit:Busy", ((LogCommand)commands[0]).Message);
        Assert.Equal("Exit:Active", ((LogCommand)commands[1]).Message);
        Assert.Equal("Transition:Busy", ((LogCommand)commands[2]).Message);
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
        Assert.Equal("Exit:Idle", ((LogCommand)commands[0]).Message);
        Assert.Equal("Transition:Idle", ((LogCommand)commands[1]).Message);
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
                    .SubStateOf(WorkState.Active);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Execute_OverloadsSupportMissingArguments()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
                .On(OrderTrigger.Pay)
                    .Execute(() => new LogCommand("NoArgs"))
                    .Execute((OrderTrigger trigger) => new LogCommand($"Trigger:{trigger}"))
                    .Execute(state => new LogCommand($"State:{state.Value}"))
                    .Execute((state, trigger) => new LogCommand($"Both:{state.Value}:{trigger}"))
            .Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-700", "none"));

        var (_, commands) = machine.Fire(OrderTrigger.Pay, current);

        Assert.Equal(4, commands.Count);
    }

    private enum OrderState
    {
        Created,
        Paid,
        Cancelled
    }

    private abstract record OrderTrigger
    {
        public sealed record PayTrigger : OrderTrigger;
        public sealed record CancelTrigger : OrderTrigger;

        public static readonly OrderTrigger Pay = new PayTrigger();
        public static readonly OrderTrigger Cancel = new CancelTrigger();
    }

    private sealed record OrderData(string OrderId, string LastEvent);

    private abstract record TestCommand;

    private sealed record ChargeCommand(string OrderId) : TestCommand;

    private sealed record LogCommand(string Message) : TestCommand;

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

    private sealed record WorkerData(int Count);
}
