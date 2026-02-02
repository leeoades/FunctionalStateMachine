using FunctionalStateMachine.Core;
using FunctionalStateMachine.Diagrams;
using Xunit.Abstractions;

namespace FunctionalStateMachine.Samples;

public static class ShoppingTrolleySample
{
[StateMachineDiagram("diagrams/ShoppingTrolley.md")]
    public static StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> Build()
    {
        return StateMachine<ShopState, CartTrigger, CartSession, ShopCommand>.Create()
            .StartWith(ShopState.Outside)
            .For(ShopState.InStore)
                .StartsWith(ShopState.Shopping)
                .On<CartTrigger.CancelTrigger>()
                    .ModifyData(data => data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Outside)
                
            .For(ShopState.Outside)
                .On<CartTrigger.StartShoppingTrigger>()
                    .ModifyData(data => data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Shopping)
            .For(ShopState.Shopping)
                .SubStateOf(ShopState.InStore)
                .On<CartTrigger.AddItemTrigger>()
                    .ModifyData((data, trigger) =>
                    {
                        var items = new List<LineItem>(data.Shop.Items);
                        items.Add(trigger.Item);

                        return data with { Shop = data.Shop with { Items = items } };
                    })
                    .Execute(data => new ShopCommand.UpdateCartItems(data.Shop.Items))
                .On<CartTrigger.RemoveItemTrigger>()
                    .ModifyData((data, trigger) =>
                    {
                        var items = new List<LineItem>(data.Shop.Items);
                        items.RemoveAll(item => item.Sku == trigger.Sku);

                        return data with { Shop = data.Shop with { Items = items } };
                    })
                    .Execute(data => new ShopCommand.UpdateCartItems(data.Shop.Items))
                .On<CartTrigger.GoToCheckoutTrigger>()
                    .TransitionTo(ShopState.CheckingOut)
            .For(ShopState.CheckingOut)
                .SubStateOf(ShopState.InStore)
                .OnEntry(data => new ShopCommand.RequestPayment(data.Shop.TotalPrice()))
                .On<CartTrigger.PayTrigger>()
                    .TransitionTo(ShopState.PaymentPending)
                    .Execute(() => new ShopCommand.DisplayPaymentPendingMessage())
                .On<CartTrigger.PayByCashTrigger>()
                    .Guard((data, trigger) => trigger.Amount + data.Shop.AmountPaid < data.Shop.TotalPrice())
                    .ModifyData((data, trigger) => data with { Shop = data.Shop with { AmountPaid = data.Shop.AmountPaid + trigger.Amount } })
                    .Execute(data =>
                    {
                        var remaining = data.Shop.TotalPrice() - data.Shop.AmountPaid;
                        return new ShopCommand.RequestPayment(remaining);
                    })
                .On<CartTrigger.PayByCashTrigger>()
                    .Guard((data, trigger) => trigger.Amount + data.Shop.AmountPaid >= data.Shop.TotalPrice())
                    .Execute(data => new ShopCommand.GrantItemOwnership(data.Shop.Items))
                    .If((data, trigger) => trigger.Amount + data.Shop.AmountPaid > data.Shop.TotalPrice())
                        .Execute((data, trigger) =>
                        {
                            var refund = trigger.Amount + data.Shop.AmountPaid - data.Shop.TotalPrice();
                            return new ShopCommand.RefundCash(refund);
                        })
                        .Done()
                    .ModifyData(data => data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Outside)
            .For(ShopState.PaymentPending)
                .SubStateOf(ShopState.InStore)
                .On<CartTrigger.PaymentSucceededTrigger>()
                    .Execute(data => new ShopCommand.GrantItemOwnership(data.Shop.Items))
                    .ModifyData(data => data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Outside)
                .On<CartTrigger.PaymentFailedTrigger>()
                    .TransitionTo(ShopState.CheckingOut)
                    .Execute(data =>
                    [
                        new ShopCommand.DisplayPaymentFailedMessage(),
                        new ShopCommand.RequestPayment(data.Shop.TotalPrice())
                    ])
            .Build();
    }
}

public enum ShopState
{
    Outside,
    InStore,
    Shopping,
    CheckingOut,
    PaymentPending
}

public sealed record CartSession(ShopData Shop)
{
    public static CartSession Initial => new(ShopData.Initial);
}

public sealed record ShopData(List<LineItem> Items, decimal AmountPaid)
{
    public decimal TotalPrice() => Items.Sum(item => item.Price);
    
    public static ShopData Initial => new([], 0);
}

public sealed record LineItem(string Sku, decimal Price);

public abstract record CartTrigger
{
    public sealed record StartShoppingTrigger : CartTrigger;
    public sealed record AddItemTrigger(LineItem Item) : CartTrigger;
    public sealed record RemoveItemTrigger(string Sku) : CartTrigger;
    public sealed record GoToCheckoutTrigger : CartTrigger;
    public sealed record PayTrigger : CartTrigger;
    public sealed record PayByCashTrigger(decimal Amount) : CartTrigger;
    public sealed record PaymentFailedTrigger : CartTrigger;
    public sealed record PaymentSucceededTrigger : CartTrigger;
    public sealed record CancelTrigger : CartTrigger;

    private static readonly CartTrigger StartShoppingInstance = new StartShoppingTrigger();
    private static readonly CartTrigger GoToCheckoutInstance = new GoToCheckoutTrigger();
    private static readonly CartTrigger PayInstance = new PayTrigger();
    private static readonly CartTrigger PaymentFailedInstance = new PaymentFailedTrigger();
    private static readonly CartTrigger PaymentSucceededInstance = new PaymentSucceededTrigger();
    private static readonly CartTrigger CancelInstance = new CancelTrigger();

    public static CartTrigger StartShopping() => StartShoppingInstance;

    public static CartTrigger AddItem(LineItem item) => new AddItemTrigger(item);

    public static CartTrigger RemoveItem(string sku) => new RemoveItemTrigger(sku);

    public static CartTrigger GoToCheckout() => GoToCheckoutInstance;

    public static CartTrigger Pay() => PayInstance;

    public static CartTrigger PayByCash(decimal amount) => new PayByCashTrigger(amount);

    public static CartTrigger PaymentFailed() => PaymentFailedInstance;

    public static CartTrigger PaymentSucceeded() => PaymentSucceededInstance;

    public static CartTrigger Cancel() => CancelInstance;
}

public abstract record ShopCommand
{
    public sealed record UpdateCartItems(IReadOnlyList<LineItem> Items) : ShopCommand;

    public sealed record RequestPayment(decimal Total) : ShopCommand;

    public sealed record DisplayPaymentPendingMessage : ShopCommand;

    public sealed record DisplayPaymentFailedMessage : ShopCommand;

    public sealed record GrantItemOwnership(IReadOnlyList<LineItem> Items) : ShopCommand;

    public sealed record RefundCash(decimal Amount) : ShopCommand;
}

public class ShoppingTrolleyDemo(ITestOutputHelper output)
{
    [Fact]
    public void Demo_Card_Payment()
    {
        var machine = ShoppingTrolleySample.Build();
        var currentState = machine.InitialStateOrDefault();
        var currentData = new CartSession(new ShopData([], 0));

        (currentState, currentData) = Fire(CartTrigger.StartShopping(), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.GoToCheckout(), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.Pay(), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.PaymentSucceeded(), currentState, currentData, machine);
    }
    
    [Fact]
    public void Demo_Cash_Payment()
    {
        var machine = ShoppingTrolleySample.Build();
        var currentState = machine.InitialStateOrDefault();
        var currentData = new CartSession(new ShopData([], 0));

        (currentState, currentData) = Fire(CartTrigger.StartShopping(), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.GoToCheckout(), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.PayByCash(1), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.PayByCash(1), currentState, currentData, machine);
        (currentState, currentData) = Fire(CartTrigger.PayByCash(0.50m), currentState, currentData, machine);
    }

    private (ShopState State, CartSession Data) Fire(
        CartTrigger trigger,
        ShopState state,
        CartSession data,
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        var (newState, newData, commands) = machine.Fire(trigger, state, data);
        Run(commands);
        return (newState, newData);
    }

    private void Run(IReadOnlyList<ShopCommand> commands)
    {
        foreach (var command in commands)
        {
            Print(command switch        
            {
                ShopCommand.UpdateCartItems update => $"Updated cart items: {string.Join(", ", update.Items)}",
                ShopCommand.RequestPayment request => $"Requesting payment for {request.Total}",
                ShopCommand.DisplayPaymentPendingMessage => "Payment pending...",
                ShopCommand.DisplayPaymentFailedMessage => "Payment failed...",
                ShopCommand.RefundCash refund => $"Refunding {refund.Amount}...",
                ShopCommand.GrantItemOwnership grant => $"Purchase complete of {string.Join(", ", grant.Items)}",
                _ => throw new ArgumentOutOfRangeException()
            });
        }
    }
    
    private void Print(string s) => output.WriteLine(s);
}
