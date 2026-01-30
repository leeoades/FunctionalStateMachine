using FunctionalStateMachine;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineTests
{
    [Fact]
    public void Fire_ReturnsNewStateAndCommands()
    {
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .StartWith(OrderState.Created);

        builder.For(OrderState.Created)
            .On(OrderTrigger.Pay)
                .TransitionTo(OrderState.Paid)
                .Execute(state => new ChargeCommand(state.Data.OrderId));

        var machine = builder.Build();
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
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create();

        builder.For(OrderState.Created)
            .OnExit(state => new LogCommand($"Exit:{state.Value}"))
            .On(OrderTrigger.Pay)
                .TransitionTo(OrderState.Paid)
                .Execute(state => new LogCommand($"Transition:{state.Value}"));

        builder.For(OrderState.Paid)
            .OnEntry(state => new LogCommand($"Entry:{state.Value}"));

        var machine = builder.Build();
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
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create();
        var created = builder.For(OrderState.Created);

        created.On(OrderTrigger.Pay)
            .Guard((state, trigger) => state.Data.OrderId.StartsWith("B"))
            .TransitionTo(OrderState.Cancelled);

        created.On(OrderTrigger.Pay)
            .Guard((state, trigger) => state.Data.OrderId.StartsWith("A"))
            .TransitionTo(OrderState.Paid);

        var machine = builder.Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-300", "none"));
        var (next, _) = machine.Fire(OrderTrigger.Pay, current);

        Assert.Equal(OrderState.Paid, next.Value);
    }

    [Fact]
    public void Fire_UpdatesDataWhenConfigured()
    {
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create();

        builder.For(OrderState.Created)
            .On(OrderTrigger.Pay)
                .WithData(state => state.Data with { LastEvent = "paid" })
                .TransitionTo(OrderState.Paid);

        var machine = builder.Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-400", "none"));
        var (next, _) = machine.Fire(OrderTrigger.Pay, current);

        Assert.Equal("paid", next.Data.LastEvent);
    }

    [Fact]
    public void Fire_IgnoresTriggersWhenConfigured()
    {
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create();

        builder.For(OrderState.Created)
            .On(OrderTrigger.Cancel)
                .Ignore();

        var machine = builder.Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-500", "none"));
        var (next, commands) = machine.Fire(OrderTrigger.Cancel, current);

        Assert.Equal(OrderState.Created, next.Value);
        Assert.Empty(commands);
    }

    [Fact]
    public void Fire_ThrowsWhenUnhandled()
    {
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create();
        builder.For(OrderState.Created);

        var machine = builder.Build();
        var current = new State<OrderState, OrderData>(OrderState.Created, new OrderData("A-600", "none"));

        Assert.Throws<InvalidOperationException>(() => machine.Fire(OrderTrigger.Pay, current));
    }

    [Fact]
    public void Fire_DelegatesToSubStateMachine()
    {
        var workerBuilder = StateMachine<WorkerState, WorkerTrigger, WorkerData, TestCommand>.Create();

        workerBuilder.For(WorkerState.Idle)
            .On(WorkerTrigger.StartWork)
                .TransitionTo(WorkerState.Busy)
                .Execute(() => new LogCommand("Start"));

        workerBuilder.For(WorkerState.Busy)
            .On(WorkerTrigger.CompleteWork)
                .TransitionTo(WorkerState.Idle)
                .Execute(() => new LogCommand("Complete"));

        var workerMachine = workerBuilder.Build();
        var builder = StateMachine<ParentState, WorkerTrigger, ParentData, TestCommand>.Create();

        builder.For(ParentState.Active)
            .WithSubStateMachine(
                workerMachine,
                data => data.Worker,
                (data, sub) => data with { Worker = sub })
            .On(WorkerTrigger.CompleteWork)
                .TransitionTo(ParentState.Closed);

        builder.For(ParentState.Closed);

        var machine = builder.Build();
        var parentData = new ParentData(new SubState<WorkerState, WorkerData>(WorkerState.Idle, new WorkerData(0)));
        var current = new State<ParentState, ParentData>(ParentState.Active, parentData);

        var (next, commands) = machine.Fire(WorkerTrigger.StartWork, current);

        Assert.Equal(ParentState.Active, next.Value);
        Assert.Equal(WorkerState.Busy, next.Data.Worker.Value);
        Assert.Single(commands);
        Assert.IsType<LogCommand>(commands[0]);
    }

    [Fact]
    public void Execute_OverloadsSupportMissingArguments()
    {
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create();

        builder.For(OrderState.Created)
            .On(OrderTrigger.Pay)
                .Execute(() => new LogCommand("NoArgs"))
                .Execute((OrderTrigger trigger) => new LogCommand($"Trigger:{trigger}"))
                .Execute((State<OrderState, OrderData> state) => new LogCommand($"State:{state.Value}"))
                .Execute((state, trigger) => new LogCommand($"Both:{state.Value}:{trigger}"));

        var machine = builder.Build();
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

    private enum OrderTrigger
    {
        Pay,
        Cancel
    }

    private sealed record OrderData(string OrderId, string LastEvent);

    private abstract record TestCommand;

    private sealed record ChargeCommand(string OrderId) : TestCommand;

    private sealed record LogCommand(string Message) : TestCommand;

    private enum ParentState
    {
        Active,
        Closed
    }

    private enum WorkerState
    {
        Idle,
        Busy
    }

    private enum WorkerTrigger
    {
        StartWork,
        CompleteWork
    }

    private sealed record WorkerData(int Count);

    private sealed record ParentData(SubState<WorkerState, WorkerData> Worker);
}
