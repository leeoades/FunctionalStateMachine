using FunctionalStateMachine;
using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class OrderProcessingSample
{
    public static StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand> Build()
    {
        var builder = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
            .StartWith(OrderState.Created);

        builder.For(OrderState.Created)
            .OnEntry(state => new AuditCommand($"Entering {state.Value}"))
            .OnExit(state => new AuditCommand($"Leaving {state.Value}"))
            .On(OrderTrigger.Pay)
                .TransitionTo(OrderState.Paid)
                .Execute(state => new ChargeCommand(state.Data.OrderId))
            .For(OrderState.Paid)
                .On(OrderTrigger.Ship)
                    .TransitionTo(OrderState.Shipped)
                    .Execute(state => new ShipCommand(state.Data.OrderId));

        return builder.Build();
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

public abstract record OrderCommand;

public sealed record AuditCommand(string Message) : OrderCommand;

public sealed record ChargeCommand(string OrderId) : OrderCommand;

public sealed record ShipCommand(string OrderId) : OrderCommand;
