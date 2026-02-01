namespace FunctionalStateMachine.Core.Tests;

public class StateMachineFireTests
{
    [Fact]
    public void Fire_ReturnsNewStateAndCommands()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .StartWith(OrderState.Created)
            .For(OrderState.Created)
                .On(OrderTrigger.Pay)
                    .TransitionTo(OrderState.Paid)
                    .Execute(data => new ChargeCommand(data.OrderId))
            .For(OrderState.Paid)
            .Build();
        var currentState = OrderState.Created;
        var currentData = new OrderData("A-100", "none");
        var (nextState, nextData, commands) = machine.Fire(OrderTrigger.Pay, currentState, currentData);

        Assert.Equal(OrderState.Paid, nextState);
        Assert.Equal("A-100", nextData.OrderId);
        Assert.Single(commands);
        Assert.IsType<ChargeCommand>(commands[0]);
    }

    [Fact]
    public void Fire_UsesGuardsInOrder()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
                .On(OrderTrigger.Pay)
                    .Guard(data => data.OrderId.StartsWith("B"))
                    .TransitionTo(OrderState.Cancelled)
                .On(OrderTrigger.Pay)
                    .Guard(data => data.OrderId.StartsWith("A"))
                    .TransitionTo(OrderState.Paid)
            .For(OrderState.Cancelled)
            .For(OrderState.Paid)
            .Build();
        var currentState = OrderState.Created;
        var currentData = new OrderData("A-300", "none");
        var (nextState, _, _) = machine.Fire(OrderTrigger.Pay, currentState, currentData);

        Assert.Equal(OrderState.Paid, nextState);
    }

    [Fact]
    public void Fire_UpdatesDataWhenConfigured()
    {
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, TestCommand>.Create()
            .For(OrderState.Created)
                .On(OrderTrigger.Pay)
                    .ModifyData(data => data with { LastEvent = "paid" })
                    .TransitionTo(OrderState.Paid)
            .For(OrderState.Paid)
            .Build();
        var currentState = OrderState.Created;
        var currentData = new OrderData("A-400", "none");
        var (_, nextData, _) = machine.Fire(OrderTrigger.Pay, currentState, currentData);

        Assert.Equal("paid", nextData.LastEvent);
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

    private sealed record OrderData(string OrderId, string LastEvent)
    {
        public static OrderData Initial => new(string.Empty, string.Empty);
    }

    private abstract record TestCommand;

    private sealed record ChargeCommand(string OrderId) : TestCommand;
}
