using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class OrderProcessingSample
{
    public static StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand> Build()
    {
        return StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
            .StartWith(OrderState.Created)
            .For(OrderState.Created)
                .OnEntry(state => new OrderCommand.Audit($"Entering {state.Value}"))
                .OnExit(state => new OrderCommand.Audit($"Leaving {state.Value}"))
                .On(OrderTrigger.Pay)
                    .TransitionTo(OrderState.Paid)
                    .Execute(state => new OrderCommand.Charge(state.Data.OrderId))
                .For(OrderState.Paid)
                    .On(OrderTrigger.Ship)
                        .TransitionTo(OrderState.Shipped)
                        .Execute(state => new OrderCommand.Ship(state.Data.OrderId))
            .Build();
    }
}

public enum OrderState
{
    Created,
    Paid,
    Shipped
}

public enum OrderTrigger
{
    Pay,
    Ship
}

public sealed record OrderData(string OrderId);

public abstract record OrderCommand
{
    public sealed record Audit(string Message) : OrderCommand;
    public sealed record Charge(string OrderId) : OrderCommand;
    public sealed record Ship(string OrderId) : OrderCommand;
}
