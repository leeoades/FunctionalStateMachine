namespace FunctionalStateMachine.FormattingTest;

using FunctionalStateMachine.Core;

/// <summary>
/// This file demonstrates the recommended fluent API formatting pattern.
/// When formatted in JetBrains Rider with the .editorconfig settings,
/// this hierarchical indentation should be preserved automatically.
/// </summary>
public class FluentApiFormattingExample
{
    public static StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand> BuildOrderStateMachine()
    {
        // Note the hierarchical indentation pattern:
        // - .StartWith/.For/.Build at +4 spaces
        // - .OnEntry/.OnExit/.On at +8 spaces  
        // - .Guard/.TransitionTo/.Execute/.ModifyData at +12 spaces
        var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
            .StartWith(OrderState.Draft)
            .For(OrderState.Draft)
                .OnEntry(() => new OrderCommand.LogStateEntry("Draft"))
                .OnExit(() => new OrderCommand.LogStateExit("Draft"))
                .On<OrderTrigger.Submit>()
                    .Guard(data => data.TotalAmount > 0)
                    .ModifyData(data => data with { SubmittedAt = DateTime.UtcNow })
                    .TransitionTo(OrderState.Pending)
                    .Execute(data => new OrderCommand.NotifyCustomer(data.OrderId, "submitted"))
            .For(OrderState.Pending)
                .OnEntry(() => new OrderCommand.LogStateEntry("Pending"))
                .On<OrderTrigger.Approve>()
                    .TransitionTo(OrderState.Approved)
                    .Execute(() => new OrderCommand.SendApprovalEmail())
                .On<OrderTrigger.Reject>()
                    .ModifyData(data => data with { RejectedAt = DateTime.UtcNow })
                    .TransitionTo(OrderState.Rejected)
                    .Execute(data => new OrderCommand.NotifyCustomer(data.OrderId, "rejected"))
            .For(OrderState.Approved)
                .On<OrderTrigger.Ship>()
                    .ModifyData(data => data with { ShippedAt = DateTime.UtcNow })
                    .TransitionTo(OrderState.Shipped)
                    .Execute(data => new OrderCommand.ArrangeShipping(data.OrderId))
            .For(OrderState.Shipped)
                .On<OrderTrigger.Deliver>()
                    .TransitionTo(OrderState.Delivered)
                    .Execute(() => new OrderCommand.MarkAsDelivered())
            .For(OrderState.Rejected)
            .For(OrderState.Delivered)
            .Build();

        return machine;
    }

    public static StateMachine<PaymentState, PaymentTrigger, PaymentData, PaymentCommand> BuildPaymentStateMachineWithConditionals()
    {
        // Demonstrates conditional branching with proper indentation
        var machine = StateMachine<PaymentState, PaymentTrigger, PaymentData, PaymentCommand>.Create()
            .StartWith(PaymentState.Pending)
            .For(PaymentState.Pending)
                .On<PaymentTrigger.Process>()
                    .If((data, trigger) => trigger.Amount > 1000m)
                        .Execute(() => new PaymentCommand.RequestManagerApproval())
                        .TransitionTo(PaymentState.RequiresApproval)
                        .ElseIf((data, trigger) => trigger.Amount > 100m)
                        .ModifyData((data, trigger) => data with { Amount = trigger.Amount })
                        .Execute(() => new PaymentCommand.ProcessStandardPayment())
                        .TransitionTo(PaymentState.Processing)
                        .Else()
                        .ModifyData((data, trigger) => data with { Amount = trigger.Amount })
                        .Execute(() => new PaymentCommand.ProcessQuickPayment())
                        .TransitionTo(PaymentState.Completed)
                        .Done()
            .For(PaymentState.RequiresApproval)
                .On<PaymentTrigger.Approve>()
                    .TransitionTo(PaymentState.Processing)
                    .Execute(() => new PaymentCommand.ProcessLargePayment())
                .On<PaymentTrigger.Deny>()
                    .TransitionTo(PaymentState.Denied)
            .For(PaymentState.Processing)
                .On<PaymentTrigger.Complete>()
                    .TransitionTo(PaymentState.Completed)
            .For(PaymentState.Completed)
            .For(PaymentState.Denied)
            .Build();

        return machine;
    }
}

// Supporting types for the examples
public enum OrderState { Draft, Pending, Approved, Rejected, Shipped, Delivered }

public abstract record OrderTrigger
{
    public sealed record Submit : OrderTrigger;
    public sealed record Approve : OrderTrigger;
    public sealed record Reject : OrderTrigger;
    public sealed record Ship : OrderTrigger;
    public sealed record Deliver : OrderTrigger;
}

public sealed record OrderData(
    string OrderId,
    decimal TotalAmount,
    DateTime? SubmittedAt = null,
    DateTime? RejectedAt = null,
    DateTime? ShippedAt = null);

public abstract record OrderCommand
{
    public sealed record LogStateEntry(string StateName) : OrderCommand;
    public sealed record LogStateExit(string StateName) : OrderCommand;
    public sealed record NotifyCustomer(string OrderId, string Status) : OrderCommand;
    public sealed record SendApprovalEmail : OrderCommand;
    public sealed record ArrangeShipping(string OrderId) : OrderCommand;
    public sealed record MarkAsDelivered : OrderCommand;
}

public enum PaymentState { Pending, RequiresApproval, Processing, Completed, Denied }

public abstract record PaymentTrigger
{
    public sealed record Process(decimal Amount) : PaymentTrigger;
    public sealed record Approve : PaymentTrigger;
    public sealed record Deny : PaymentTrigger;
    public sealed record Complete : PaymentTrigger;
}

public sealed record PaymentData(decimal Amount);

public abstract record PaymentCommand
{
    public sealed record RequestManagerApproval : PaymentCommand;
    public sealed record ProcessStandardPayment : PaymentCommand;
    public sealed record ProcessQuickPayment : PaymentCommand;
    public sealed record ProcessLargePayment : PaymentCommand;
}
