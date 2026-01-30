using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class OrderProcessingSample
{
    public static StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand> Build()
    {
        return StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
            .StartWith(OrderState.Created)
            .For(OrderState.Created)
                .OnEntry(state => new OrderCommand.RecordAudit($"Entering {state.Value}"))
                .OnExit(state => new OrderCommand.RecordAudit($"Leaving {state.Value}"))
                .On(OrderTrigger.Pay)
                    .TransitionTo(OrderState.Paid)
                    .Execute(state => new OrderCommand.ChargePayment(state.Data.OrderId))
                .For(OrderState.Paid)
                    .On(OrderTrigger.Ship)
                        .TransitionTo(OrderState.Shipped)
                        .Execute(state => new OrderCommand.ShipOrder(state.Data.OrderId))
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
    public sealed record RecordAudit(string Message) : OrderCommand;
    public sealed record ChargePayment(string OrderId) : OrderCommand;
    public sealed record ShipOrder(string OrderId) : OrderCommand;
}
